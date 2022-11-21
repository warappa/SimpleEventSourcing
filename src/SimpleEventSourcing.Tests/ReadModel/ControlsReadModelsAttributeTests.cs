using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.State;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Tests.ReadModel
{
    [TestFixture]
    public class ControlsReadModelsAttributeTests
    {
        [Test]
        public void GetStateTypeForReadModel_returns_state_in_provided_search_assemblies()
        {
            var foundState = ControlsReadModelsAttribute.GetProjectorTypeForReadModel(typeof(StateReadModel), typeof(State).Assembly);
            foundState.Should().Be(typeof(State));
        }

        [Test]
        public void GetStateTypeForReadModel_without_provided_searchassemblies_and_without_previous_known_assemblies_should_throw_InvalidOperationException()
        {
            ControlsReadModelsAttribute.ClearKnownAssemblies();

            ((Action)(() => ControlsReadModelsAttribute.GetProjectorTypeForReadModel(typeof(StateReadModel)))).Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void GetStateTypeForReadModel_without_provided_searchassemblies_but_known_assemblies_should_return_state_type()
        {
            ControlsReadModelsAttribute.ClearKnownAssemblies();

            ControlsReadModelsAttribute.GetProjectorTypeForReadModel(typeof(StateReadModel), typeof(State).Assembly);

            Type foundState = null;
            ((Action)(() => foundState = ControlsReadModelsAttribute.GetProjectorTypeForReadModel(typeof(StateReadModel)))).Should().NotThrow<InvalidOperationException>();

            foundState.Should().Be(typeof(State));
        }

        [Test]
        public void GetStateTypeForReadModel_with_readmodel_claimed_to_be_controlled_by_multiple_states_should_throw_InvalidOperationException()
        {
            ControlsReadModelsAttribute.ClearKnownAssemblies();

            ((Action)(() => ControlsReadModelsAttribute.GetProjectorTypeForReadModel(typeof(StateReadModelClaimedByMultiple), typeof(State).Assembly))).Should().Throw<InvalidOperationException>();
        }

        [ControlsReadModels(new[] { typeof(StateReadModel) })]
        public class State : IAsyncProjector
        {
            public Type[] PayloadTypes => throw new NotImplementedException();

            public object UntypedApply(object eventOrMessage)
            {
                throw new NotImplementedException();
            }

            public async Task<object> UntypedApplyAsync(object eventOrMessage)
            {
                throw new NotImplementedException();
            }
        }

        public class StateReadModel : IReadModel<int>
        {
            public int Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            object IReadModelBase.Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }

        [ControlsReadModels(new[] { typeof(StateReadModelClaimedByMultiple) })]
        public class StateClaimingA : IAsyncProjector
        {
            public Type[] PayloadTypes => throw new NotImplementedException();

            public object UntypedApply(object eventOrMessage)
            {
                throw new NotImplementedException();
            }

            public async Task<object> UntypedApplyAsync(object eventOrMessage)
            {
                throw new NotImplementedException();
            }
        }

        [ControlsReadModels(new[] { typeof(StateReadModelClaimedByMultiple) })]
        public class StateClaimingB : IAsyncProjector
        {
            public Type[] PayloadTypes => throw new NotImplementedException();

            public object UntypedApply(object eventOrMessage)
            {
                throw new NotImplementedException();
            }

            public async Task<object> UntypedApplyAsync(object eventOrMessage)
            {
                throw new NotImplementedException();
            }
        }

        public class StateReadModelClaimedByMultiple : IReadModel<int>
        {
            public int Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            object IReadModelBase.Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }
    }
}
