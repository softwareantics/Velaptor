using System.Collections.Generic;
using Silk.NET.OpenGL;
using Moq;
using Raptor.OpenGL;
using Xunit;
using System.Linq;

namespace RaptorTests.OpenGL
{
    /// <summary>
    /// Initializes a new instance of <see cref="GPUBufferTests"/>.
    /// </summary>
    public class GPUBufferTests
    {
        private readonly Mock<IGLInvoker> _mockGL;
        private readonly uint _vertexArrayID = 1256;
        private readonly uint _vertexBufferID = 1234;
        private readonly uint _indexBufferID = 5678;
        private bool _vertexBufferCreated;
        private bool _indexBufferCreated;

        public GPUBufferTests()
        {
            _mockGL = new Mock<IGLInvoker>();
            _mockGL.Setup(m => m.GenBuffer()).Returns(() =>
            {
                if (!_vertexBufferCreated)
                {
                    _vertexBufferCreated = true;
                    return _vertexBufferID;
                }

                if (!_indexBufferCreated)
                {
                    _indexBufferCreated = true;
                    return _indexBufferID;
                }

                return 0;
            });
            _mockGL.Setup(m => m.GenVertexArray()).Returns(_vertexArrayID);
        }

        [Fact]
        public void Ctor_WhenInvoked_CreatesVertexBuffer()
        {
            //Act
            var buffer = new GPUBuffer<VertexData>(_mockGL.Object)
            {
                TotalQuads = 2
            };

            //Assert
            //These are all invoked once per quad.  The number of invokes is 4 because
            //the internal Init() method that invokes these is called once in the constructor
            //as well as when the setter of the TotalQuads property is invoked.
            _mockGL.Verify(m => m.GenBuffer(), Times.AtLeastOnce());

            _mockGL.Verify(m => m.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferID), Times.AtLeast(2));

            //TODO: Get this verify to work again
            _mockGL.Verify(m => m.BufferData(BufferTargetARB.ArrayBuffer, It.IsAny<uint>(), BufferUsageARB.DynamicDraw), Times.AtLeastOnce());
            _mockGL.Verify(m => m.BindBuffer(BufferTargetARB.ArrayBuffer, 0), Times.AtLeast(2));//Unbind check
        }
        
        [Fact]
        public void Ctor_WhenInvoked_CreatesIndexBuffer()
        {
            //Act
            _mockGL.Setup(m => m.BufferData(BufferTargetARB.ElementArrayBuffer, It.IsAny<uint>(), It.IsAny<uint[]>(), BufferUsageARB.DynamicDraw));

            var buffer = new GPUBuffer<VertexData>(_mockGL.Object);

            //Assert
            //First invoke is done creating the Vertex Buffer, the second is the index buffer
            _mockGL.Verify(m => m.GenBuffer(), Times.Exactly(2));
            _mockGL.Verify(m => m.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBufferID), Times.Exactly(2));
            _mockGL.Verify(m => m.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0), Times.Exactly(1));
        }

        [Fact]
        public void Ctor_WhenInvoking_SetsUpAttributes()
        {
            //Arrange
            var invokeCount = typeof(VertexData).GetFields().Count();
            var actualIndices = new List<uint>();
            var actualSizes = new List<int>();
            var actualOffsets = new List<uint>();

            //Collect the various parameter values for assert comparisons later
            _mockGL.Setup(m => m.VertexAttribPointer(It.IsAny<uint>(), It.IsAny<int>(), VertexAttribPointerType.Float, false, It.IsAny<uint>(), It.IsAny<uint>()))
                .Callback<uint, int, VertexAttribPointerType, bool, uint, uint>((index, size, type, normalized, stride, offset) =>
                {
                    actualIndices.Add(index);
                    actualSizes.Add(size);
                    actualOffsets.Add(offset);
                });

            //Act
            var buffer = new GPUBuffer<VertexData>(_mockGL.Object);

            //Assert
            _mockGL.Verify(m => m.EnableVertexArrayAttrib(_vertexArrayID, It.IsAny<uint>()), Times.Exactly(invokeCount));
            _mockGL.Verify(m => m.VertexAttribPointer(
                It.IsAny<uint>(),
                It.IsAny<int>(),
                VertexAttribPointerType.Float,
                false,
                40,
                It.IsAny<uint>()),
            Times.Exactly(invokeCount));

            Assert.Equal(new uint[] { 0, 1, 2, 3 }, actualIndices.ToArray());
            Assert.Equal(new int[] { 3, 2, 4, 1 }, actualSizes.ToArray());
            Assert.Equal(new uint[] { 0, 12, 20, 36 }, actualOffsets.ToArray());
        }

        [Fact]
        public void TotalQuads_WhenSettingValue_ReturnsCorrectValue()
        {
            //Arrange
#pragma warning disable IDE0017 // Simplify object initialization
            var buffer = new GPUBuffer<VertexData>(_mockGL.Object);
#pragma warning restore IDE0017 // Simplify object initialization

            //Act
            buffer.TotalQuads = 5;

            //Assert
            Assert.Equal(5, buffer.TotalQuads);
        }

        //[Fact]
        //public void UpdateQuad_WhenInvoked_UpdatesGPUVertexBuffer()
        //{
        //    //Arrange
        //    var buffer = new GPUBuffer<VertexData>(_mockGL.Object);
        //    var srcRect = new Rectangle();

        //    //Act
        //    buffer.UpdateQuad(0, srcRect, 50, 50, Color.White);
            
        //    //Assert
        //    _mockGL.Verify(m => m.BufferSubData(BufferTarget.ArrayBuffer, It.IsAny<IntPtr>(), It.IsAny<int>(), It.IsAny<QuadData>()));
        //}

        [Fact]
        public void Dispose_WithUnmanagedResourcesToDispose_DisposesOfUnmanagedResources()
        {
            //Arrange
            var buffer = new GPUBuffer<VertexData>(_mockGL.Object);

            //Act
            buffer.Dispose();
            buffer.Dispose();

            //Assert
            _mockGL.Verify(m => m.DeleteVertexArray(_vertexArrayID), Times.Once());
            _mockGL.Verify(m => m.DeleteBuffer(_vertexBufferID), Times.Once());
            _mockGL.Verify(m => m.DeleteBuffer(_indexBufferID), Times.Once());
        }
    }
}
