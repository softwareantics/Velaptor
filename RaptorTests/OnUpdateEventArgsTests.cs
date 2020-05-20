﻿using Raptor;
using Moq;
using Xunit;

namespace RaptorTests
{
    /// <summary>
    /// Unit tests to test the <see cref="OnUpdateEventArgs"/> class.
    /// </summary>
    public class OnUpdateEventArgsTests
    {
        #region Constructor Tests
        [Fact]
        public void Ctor_WhenInvoking_SetsRendererProp()
        {
            //Arrange
            var mockRenderer = new Mock<IGameTiming>();
            var expected = mockRenderer.Object;
            var eventArgs = new OnUpdateEventArgs(mockRenderer.Object);

            //Act
            var actual = eventArgs.EngineTime;

            //Assert
            Assert.Equal(expected, actual);
        }
        #endregion


        #region Prop Tests
        [Fact]
        public void Renderer_WhenGettingAndSettingValue_ReturnsCorrectValue()
        {
            //Arrange
            var mockRendererForCtor = new Mock<IGameTiming>();
            var eventArgs = new OnUpdateEventArgs(mockRendererForCtor.Object);
            var mockRendererForProp = new Mock<IGameTiming>();
            var expected = mockRendererForProp.Object;

            //Act
            eventArgs.EngineTime = mockRendererForProp.Object;
            var actual = eventArgs.EngineTime;

            //Assert
            Assert.Equal(expected, actual);
        }
        #endregion
    }
}
