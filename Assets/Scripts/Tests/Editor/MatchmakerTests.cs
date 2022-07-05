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
        /// Pick the game mode and map which all the players share.
        /// </summary>
        [Test]
        public void AllocationPayloadToGameInfo_SharedGameInfo()
        {
            var spaceMeditationPlayer1 = NewMatchPlayer(GameQueue.Casual, Map.Space, GameMode.Meditating);

            var spaceMeditationPlayer2 = NewMatchPlayer(GameQueue.Casual, Map.Space, GameMode.Meditating);

            var playerList = new List<Player>()
            {
                spaceMeditationPlayer1,
                spaceMeditationPlayer2
            };
            var simulatedPayload = CasualDefaultAllocationPayload(playerList);

            var returnedGameInfo = ServerGameManager.PickGameInfo(simulatedPayload);

            Debug.Log($"Users shared map and mode, picked {returnedGameInfo.map} and {returnedGameInfo.gameMode}");
            Assert.IsTrue(returnedGameInfo.map == Map.Space && returnedGameInfo.gameMode == GameMode.Meditating,
                $"Users shared Space and Meditating, picked {returnedGameInfo.map} and {returnedGameInfo.gameMode}");
        }

        MatchmakingResults CasualDefaultAllocationPayload(List<Player> players)
        {
            var playerIdList = players.Select(player => player.Id).ToList();
            var teamPonder = new Team("Ponderers", FakeId(), playerIdList);

            var teamList = new List<Team>()
            {
                teamPonder
            };

            var backfillTicketId = FakeId();
            var matchProperties =
                    new MatchProperties(teams: teamList, players: players, backfillTicketId: backfillTicketId);

            //The Matchmaker constructs this for us
            return new MatchmakingResults(matchProperties, null, "casual-queue", "Default Pool", null, backfillTicketId);
        }

        Player NewMatchPlayer(GameQueue queue, Map chosenMap,
            GameMode chosenMode)
        {
            GameInfo playerGameInfo = new GameInfo
            {
                map = chosenMap,
                gameMode = chosenMode,
                gameQueue = queue,
            };

            var fakeAuthId = FakeId();

            UserData playerData = new UserData(NameGenerator.GetName(fakeAuthId), fakeAuthId, 1, playerGameInfo);
            return new Player(playerData.userName, playerData.userGamePreferences);
        }

        string FakeId() => Guid.NewGuid().ToString();
    }
}