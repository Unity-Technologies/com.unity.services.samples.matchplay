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
        ///Pick a random game mode and map
        /// </summary>
        [Test]
        public void AllocationPayloadToGameInfo_TwoAnyPlayers()
        {
            var allOptionsPlayer1 = NewRandomMatchmakingPlayer(GameQueue.Casual, (Map)3, (GameMode)3);

            var allOptionsPlayer2 = NewRandomMatchmakingPlayer(GameQueue.Casual, (Map)3, (GameMode)3);

            var playerList = new List<Player>()
            {
                allOptionsPlayer1,
                allOptionsPlayer2
            };

            var simulatedPayload = CasualDefaultAllocationPayload(playerList);

            var returnedGameInfo = ServerGameManager.PickSharedGameInfo(simulatedPayload);

            Debug.Log($"Users Shared All preferences, randomly picked {returnedGameInfo.map} and {returnedGameInfo.gameMode}");
            Assert.IsTrue(returnedGameInfo.map != Map.None && returnedGameInfo.gameMode != GameMode.None,
                $"Users Shared All preferences, randomly picked {returnedGameInfo.map} and {returnedGameInfo.gameMode}");
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

            Debug.Log($"Users Shared Lab and Staring, picked {returnedGameInfo.map} and {returnedGameInfo.gameMode}");
            Assert.IsTrue(returnedGameInfo.map == Map.Lab && returnedGameInfo.gameMode == GameMode.Staring,
                $"Users Shared Map.Lab and GameMode.Staring, picked {returnedGameInfo.map} and {returnedGameInfo.gameMode}");
        }

        /// <summary>
        /// Pick the game mode and map which all the players share.
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

            Debug.Log($"Users shared map and mode, picked {returnedGameInfo.map} and {returnedGameInfo.gameMode}");
            Assert.IsTrue(returnedGameInfo.map == Map.Space && returnedGameInfo.gameMode == GameMode.Meditating,
                $"Users shared Space and Meditating, picked {returnedGameInfo.map} and {returnedGameInfo.gameMode}");
        }

        /// <summary>
        /// Pick a random game mode and map since there is a tie in preferences
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

            Debug.Log($"Users did not share map or mode, randomly picked {returnedGameInfo.map} and {returnedGameInfo.gameMode}.");
            Assert.IsTrue(returnedGameInfo.map != Map.None && returnedGameInfo.gameMode != GameMode.None,
                $"Users did not share map or mode, randomly picked {returnedGameInfo.map} and {returnedGameInfo.gameMode}");
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

        public Player NewRandomMatchmakingPlayer(GameQueue queue, Map chosenMap = Map.None, GameMode chosenMode = GameMode.None)
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

            GameInfo randomUserGameInfo = new GameInfo
            {
                map = playerMap,
                gameMode = playerMode,
                gameQueue = queue,
            };

            var fakeAuthId = FakeId();

            UserData playerData = new UserData(NameGenerator.GetName(fakeAuthId), fakeAuthId, 1, randomUserGameInfo);
            return new Player(playerData.userName, playerData.userGamePreferences);
        }

        string FakeId() => Guid.NewGuid().ToString();
    }
}
