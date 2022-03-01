
using Matchplay.Client;
using Matchplay.Infrastructure;
using NUnit.Framework;
using UnityEngine;

namespace Matchplay.Tests
{
    public class InfrastructureTests
    {
        [Test]
        public void ClientDependencies()
        {
            var rootScope = DIScope.RootScope;
            rootScope.BindAsSingle<AuthenticationHandler>();
            rootScope.BindAsSingle<MatchplayMatchmaker>();
            rootScope.BindAsSingle<MatchplayClient>();
            rootScope.BindAsSingle<ClientGameManager>();
            rootScope.FinalizeScopeConstruction();

        }
    }
}
