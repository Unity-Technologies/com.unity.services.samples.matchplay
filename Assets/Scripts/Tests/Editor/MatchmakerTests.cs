using System;
using System.Collections.Generic;
using System.Linq;
using Matchplay.Server;
using Matchplay.Shared;
using Matchplay.Shared.Tools;
using NUnit.Framework;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Matchplay.Tests
{
    public class MatchmakerTests
    {
        /// <summary>
        ///Pick a random game mode and gameMap
        /// </summary>
        [Test]
        public void AllocationPayloadToGameInfo_TwoAnyPlayers()
        {
            var simplePayload = SimplePayload();

            var returnedGameInfo = ServerGameManager.PickSharedGameInfo(simplePayload);

            Debug.Log(
                $"Users Shared All preferences, randomly picked {returnedGameInfo.GetMap()} and {returnedGameInfo.GetMode()}");
            Assert.IsTrue(returnedGameInfo.GetMap() != Map.None && returnedGameInfo.GetMode() != GameMode.None,
                $"Users Shared All preferences, randomly picked {returnedGameInfo.GetMap()} and {returnedGameInfo.GetMode()}");
        }

        /// <summary>
        /// Pick a game mode which all the players share.
        /// </summary>
        [Test]
        public void AllocationPayloadToGameInfo_AnyPlayerAndSpecificPlayer()
        {
            var allOptionsPlayer = NewRandomMatchmakingPlayer(GameQueue.Casual, (Map)3, (GameMode)3);

            var labStaringPlayer = NewRandomMatchmakingPlayer(GameQueue.Casual, Map.Lab, GameMode.Staring);

            var playerList = new List<Player>()
            {
                allOptionsPlayer,
                labStaringPlayer
            };

            var simulatedPayload = CasualDefaultAllocationPayload(playerList);

            var returnedGameInfo = ServerGameManager.PickSharedGameInfo(simulatedPayload);

            Debug.Log(
                $"Users Shared Lab and Staring, picked {returnedGameInfo.GetMap()} and {returnedGameInfo.GetMode()}");
            Assert.IsTrue(returnedGameInfo.GetMap() == Map.Lab && returnedGameInfo.GetMode() == GameMode.Staring,
                $"Users Shared gameMap.Lab and gameMode.Staring, picked {returnedGameInfo.GetMap()} and {returnedGameInfo.GetMode()}");
        }

        /// <summary>
        /// Pick the game mode and gameMap which all the players share.
        /// </summary>
        [Test]
        public void AllocationPayloadToGameInfo_SharedGameInfo()
        {
            var spaceMeditationPlayer1 = NewRandomMatchmakingPlayer(GameQueue.Casual, Map.Space, GameMode.Meditating);

            var spaceMeditationPlayer2 = NewRandomMatchmakingPlayer(GameQueue.Casual, Map.Space, GameMode.Meditating);

            var playerList = new List<Player>()
            {
                spaceMeditationPlayer1,
                spaceMeditationPlayer2
            };
            var simulatedPayload = CasualDefaultAllocationPayload(playerList);

            var returnedGameInfo = ServerGameManager.PickSharedGameInfo(simulatedPayload);

            Debug.Log(
                $"Users shared gameMap and mode, picked {returnedGameInfo.GetMap()} and {returnedGameInfo.GetMode()}");
            Assert.IsTrue(returnedGameInfo.GetMap() == Map.Space && returnedGameInfo.GetMode() == GameMode.Meditating,
                $"Users shared Space and Meditating, picked {returnedGameInfo.GetMap()} and {returnedGameInfo.GetMode()}");
        }

        /// <summary>
        /// Pick a random game mode and gameMap since there is a tie in preferences
        /// </summary>
        [Test]
        public void AllocationPayloadToGameInfo_NoSharedGameInfo()
        {
            var spaceMeditationPlayer = NewRandomMatchmakingPlayer(GameQueue.Casual, Map.Space, GameMode.Meditating);

            var labStaringPlayer = NewRandomMatchmakingPlayer(GameQueue.Casual, Map.Lab, GameMode.Staring);

            var playerList = new List<Player>()
            {
                spaceMeditationPlayer,
                labStaringPlayer
            };
            var simulatedPayload = CasualDefaultAllocationPayload(playerList);

            var returnedGameInfo = ServerGameManager.PickSharedGameInfo(simulatedPayload);

            Debug.Log(
                $"Users did not share gameMap or mode, randomly picked {returnedGameInfo.GetMap()} and {returnedGameInfo.GetMode()}.");
            Assert.IsTrue(returnedGameInfo.GetMap() != Map.None && returnedGameInfo.GetMode() != GameMode.None,
                $"Users did not share gameMap or mode, randomly picked {returnedGameInfo.GetMap()} and {returnedGameInfo.GetMode()}");
        }

        MatchmakerAllocationPayload SimplePayload()
        {
            var allOptionsPlayer1 = NewRandomMatchmakingPlayer(GameQueue.Casual, (Map)3, (GameMode)3);

            var allOptionsPlayer2 = NewRandomMatchmakingPlayer(GameQueue.Casual, (Map)3, (GameMode)3);

            var playerList = new List<Player>()
            {
                allOptionsPlayer1,
                allOptionsPlayer2
            };

            return CasualDefaultAllocationPayload(playerList);
        }

        MatchmakerAllocationPayload CasualDefaultAllocationPayload(List<Player> players)
        {
            var playerIdList = players.Select(player => player.Id).ToList();
            var teamPonder = new Team("Ponderers", FakeId(), playerIdList);

            var teamList = new List<Team>()
            {
                teamPonder
            };

            var backfillTicketId = FakeId();

            //The Matchmaker constructs this for us
            return new MatchmakerAllocationPayload()
            {
                QueueName = "casual-queue",
                PoolName = "Default Pool",
                BackfillTicketId = backfillTicketId,
                MatchProperties = new MatchProperties(teamList, players, "REGION", backfillTicketId)
            };
        }

        public Player NewRandomMatchmakingPlayer(GameQueue queue, Map chosenMap = Map.None,
            GameMode chosenMode = GameMode.None)
        {
            Map playerMap = chosenMap;
            if (playerMap == Map.None)
            {
                playerMap = (Map)Random.Range(1, 4);
            }

            GameMode playerMode = chosenMode;
            if (playerMode == GameMode.None)
            {
                playerMode = (GameMode)Random.Range(1, 4);
            }

            GameInfo randomUserGameInfo = new GameInfo(queue, playerMap, playerMode);

            var fakeAuthId = FakeId();

            UserData playerData = new UserData(NameGenerator.GetName(fakeAuthId), fakeAuthId, 1, randomUserGameInfo);
            return new Player(playerData.userName, playerData.userGamePreferences);
        }

        string FakeId() => Guid.NewGuid().ToString();
    }
}