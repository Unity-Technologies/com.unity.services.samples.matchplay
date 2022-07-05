using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Services.Wire.Internal
{
    [Serializable]
    class CentrifugeError
    {
        public CentrifugeErrorCode code;
        public string message;
    }

    static class CommandID
    {
        private static UInt32 currentId = 0;
        public static UInt32 GenerateNewId() => ++ CommandID.currentId;
    }

    abstract class Message
    {
        public enum Method
        {
            CONNECT = 0,
            SUBSCRIBE = 1,
            UNSUBSCRIBE = 2,
            PUBLISH = 3,
            PRESENCE = 4,
            PRESENCE_STATS = 5,
            HISTORY = 6,
            PING = 7,
            SEND = 8,
            RPC = 9,
            REFRESH = 10,
            SUB_REFRESH = 11
        }
        public string Serialize()
        {
            string data = JsonConvert.SerializeObject(this);
            Logger.LogVerbose($"serializing message: {data}");
            return data;
        }

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(Serialize());
        }

        public abstract string GetMethod();
    }


    [Serializable]
    class Command<RequestClass> : Message
    {
        public UInt32 id;
        public Method method;
        public RequestClass @params;   // params is a reserved keyword but decided by the centrifuge protocol, we need @ to workaround this issue

        public Command(Method method, RequestClass @params)
        {
            this.id = CommandID.GenerateNewId();
            this.method = method;
            this.@params = @params;
        }

        public static Command<RequestClass> FromJSON(byte[] data)
        {
            return JsonUtility.FromJson<Command<RequestClass>>(Encoding.UTF8.GetString(data));
        }

        public override string GetMethod()
        {
            return method.ToString();
        }
    }

    [Serializable]
    class ConnectRequest : Message
    {
        public string token;
        //public string data;
        //public string version;
        [JsonProperty]
        public Dictionary<string, SubscribeRequest> subs;
        // public string name;

        public ConnectRequest(string token)
        {
            this.token = token;
            //this.name = "js";
            //this.version = "";
            //this.data = "";
        }

        public ConnectRequest(string token, Dictionary<string, SubscribeRequest> subscriptionRequests)
        {
            subs = subscriptionRequests;
            this.token = token;
        }

        public override string GetMethod()
        {
            throw new NotImplementedException();
        }
    }

    class SubscribeRequest : Message
    {
        public string channel;
        public string token;

        // NYI, recover/history related features
        //
        public bool recover;
        public ulong offset;
        public string epoch;

        public static async Task<Dictionary<string, SubscribeRequest>> getRequestFromRepo(ISubscriptionRepository repository)
        {
            var subscriptionRequests = new Dictionary<string, SubscribeRequest>();
            foreach (var subIterator in repository.GetAll())
            {
                var subscriptionToken = await subIterator.Value.RetrieveTokenAsync();
                subscriptionRequests.Add(subIterator.Key, new SubscribeRequest()
                {
                    recover = repository.IsRecovering(subIterator.Value),
                    token = subscriptionToken
                });
            }

            return subscriptionRequests;
        }

        public override string GetMethod()
        {
            throw new NotImplementedException();
        }
    }

    class UnsubscribeRequest : Message
    {
        public string channel;
        public override string GetMethod()
        {
            throw new NotImplementedException();
        }
    }

    class SubscribeResult
    {
        public bool expires;
        public UInt32 ttl;
        public bool recoverable;
        public string epoch;
        public bool recovered;
        public UInt64 offset;
        public bool positioned;
        public string data;
        public CentrifugePayload[] publications;
    }


    class ClientInfo
    {
        public string user;
        public string client;
        public byte[] conn_info;
        public byte[] chan_info;
    }


    // Reply should look like this in json:
    // {"id":1,"result":{"client":"42717820-8d47-4af0-96bc-07f2c36cd17d"}}
    class Reply
    {
        public UInt32 id;
        public CentrifugeError error;
        public Result result;
        public string originalString = "";

        public Reply(UInt32 id, CentrifugeError error, Result result)
        {
            this.id = id;
            this.error = error;
            this.result = result;
        }

        static public Reply FromJson(byte[] jsonData)
        {
            return FromJson(Encoding.UTF8.GetString(jsonData));
        }

        static public Reply FromJson(string jsonData)
        {
            var reply = JsonConvert.DeserializeObject<Reply>(jsonData);
            reply.originalString = jsonData;
            return reply;
        }

        public byte[] ToJson()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }

        public bool HasError()
        {
            return this.error != null && this.error.code != 0;
        }
    }

    enum PushType
    {
        PUBLICATION = 0,
        JOIN = 1,
        LEAVE = 2,
        UNSUB = 3,
        MESSAGE = 4,
        SUB = 5,
        CONNECT = 6,
        DISCONNECT = 7,
        REFRESH = 8
    }


    [Serializable]
    class Result
    {
        // connect reply members:
        public string client;
        public string version;
        public bool expires;
        public UInt32 ttl;
        public string channel;
        public Dictionary<string, SubscribeResult> subs;
        public CentrifugePayload[] publications;
        // push message members
        public PushType type;
        public CentrifugePayload data;
        // public string channel;

        // subscribe result members
        // public bool expires;
        // public UInt32 ttl;
        public bool recoverable;
        public string epoch;
        public bool recovered;
        public UInt64 offset;
        public bool positioned;

        public ConnectResult ToConnectionResult()
        {
            return new ConnectResult
            {
                client = client,
                version = version,
                expires = expires,
                ttl = ttl,
                subs = subs,
            };
        }

        public SubscribeResult ToSubscribeResult()
        {
            return new SubscribeResult
            {
                expires = expires,
                ttl = ttl,
                epoch = epoch,
                offset = offset,
                positioned = positioned,
                recoverable = recoverable,
                recovered = recovered,
                publications = publications,
            };
        }
    }

    [Serializable]
    class ConnectResult
    {
        public string client;
        public string version;
        public bool expires;
        public UInt32 ttl;
        public string data;
        public Dictionary<string, SubscribeResult> subs;
    }

    [Serializable]
    class PingRequest {}
    [Serializable]
    class PingResult {}

    [Serializable]
    class CentrifugePayload
    {
        [Preserve]
        public CentrifugePayload()
        {
        }

        public WireMessage data;
        public UInt64 offset;
    }


    static class BatchMessages
    {
        // getReplies returns the list of messages received in the websocket buffer
        // sometimes, centrifuge can batch multiple messages into one single buffer
        // ex:[Wire]: WS received message: {"result":{"channel":"$example!!!test","data":{"data":{"message":"test 6"},"offset":248}}}
        // {"result":{"channel":"$example!!!test","data":{"data":{"message":"test 1"},"offset":249}}}
        private static IEnumerable<string> SplitMessages(string message)
        {
            var publications = message.Split(new string[] { "}\n{" }, StringSplitOptions.None);

            if (publications.Length > 1)
            {
                // String.Split removes the delimiter, we need to add the curly brackets back to parse json properly.
                FixJsonSplit(ref publications);
            }

            return publications;
        }

        public static IEnumerable<string> SplitMessages(byte[] byteMessage)
        {
            // we have to use the }\n{ delimiter as the messages themselves could contains the \n symbol.
            return SplitMessages(Encoding.UTF8.GetString(byteMessage));
        }

        // fixJsonSplit fixes each individual messages created by splitMessages
        // splitMessages will split the messages and remove curly brackets at the beginning and enf of messages after split
        // ex: {"result":{"channel":"$example!!!test","data":{"data":{"message":"test 0"},"offset":248}}}
        // {"result":{"channel":"$example!!!test","data":{"data":{"message":"test 1"},"offset":249}}}
        // {"result":{"channel":"$example!!!test","data":{"data":{"message":"test 2"},"offset":250}}}
        // will become
        // [0] :  {"result":{"channel":"$example!!!test","data":{"data":{"message":"test 0"},"offset":248}}
        // [1] : "result":{"channel":"$example!!!test","data":{"data":{"message":"test 1"},"offset":249}}
        // [2] : "result":{"channel":"$example!!!test","data":{"data":{"message":"test 2"},"offset":250}}}
        private static void FixJsonSplit(ref string[] pubs)
        {
            for (var i = 0; i < pubs.Length; i++)
            {
                if (i > 0) // first message didn't match the ending regex, therefore it doesn't miss the opening {
                {
                    pubs[i] = "{" + pubs[i];
                }

                if (i < pubs.Length - 1) // the last message doesn't match the regex neither, so no need to add the closing }
                {
                    pubs[i] += "}";
                }
            }
        }
    }
}
