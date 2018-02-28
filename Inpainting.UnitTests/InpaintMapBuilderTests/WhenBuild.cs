using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NUnit.Framework;
using Rhino.Mocks;
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.UnitTests.Inpainting.InpaintMapBuilderTests
{
    [TestFixture]
    public class WhenBuild
    {
        [Test]
        public void Shoud_Throw_MapIsNotInitializedException_When_Called_Before_InitMap_Call()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.Stub<IArea2DMapBuilder>();

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            Assert.Throws<MapIsNotInitializedException>(() => inpaintMapBuilder.Build());
        }

        [Test]
        public void Should_Call_Build_Of_MapBuilder()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.Stub<IArea2DMapBuilder>();
            var imageArea = Area2D.Create(0, 0, 15, 15);
            var inpaint = Area2D.Create(3, 3, 3, 3);
            mapBuilder.Expect(mb => mb.InitNewMap(imageArea, imageArea))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.SetIgnoredSourcedArea(Arg<Area2D>.Matches(x => x.IsSameAs(inpaint))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.Build())
                      .Return(null);

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            inpaintMapBuilder.InitNewMap(imageArea);
            inpaintMapBuilder.SetInpaintArea(inpaint);
            inpaintMapBuilder.Build();

            mapBuilder.AssertWasCalled(x => x.Build());
        }

        [Test]
        public void Should_Extract_Source_And_Dest_Areas_From_Donor()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.StrictMock<IArea2DMapBuilder>();
            var imageArea = Area2D.Create(0, 0, 15, 15);

            var donor1 = Area2D.Create(0, 3, 8, 3);
            var inpaint = Area2D.Create(3, 3, 3, 3);

            var srcArea = Area2D.Create(0, 3, 3, 3).Join(Area2D.Create(6, 3, 2, 3));
            var dstArea = Area2D.Create(3, 3, 3, 3);

            mapBuilder.Expect(mb => mb.InitNewMap(imageArea, imageArea))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.SetIgnoredSourcedArea(Arg<Area2D>.Matches(x => x.IsSameAs(inpaint))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.AddAssociatedAreas(Arg<Area2D>.Matches(x => x.IsSameAs(dstArea)),
                                                          Arg<Area2D>.Matches(x => x.IsSameAs(srcArea))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.Build())
                      .Return(null);

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            inpaintMapBuilder.InitNewMap(imageArea);
            inpaintMapBuilder.AddDonor(donor1);
            inpaintMapBuilder.SetInpaintArea(inpaint);
            inpaintMapBuilder.Build();
        }

        [Test]
        public void Should_Throw_InpaintAreaIsNotSetException_When_InpaintArea_Is_Not_Set()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.StrictMock<IArea2DMapBuilder>();
            var imageArea = Area2D.Create(0, 0, 15, 15);

            var donor1 = Area2D.Create(0, 3, 8, 3);

            mapBuilder.Expect(mb => mb.InitNewMap(imageArea, imageArea))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.Build())
                      .Return(null);

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            inpaintMapBuilder.InitNewMap(imageArea);
            inpaintMapBuilder.AddDonor(donor1);
            //do not set inpaint area
            //inpaintMapBuilder.SetInpaintArea(inpaint);
            Assert.Throws<InpaintAreaIsNotSetException>(() => inpaintMapBuilder.Build());
        }

        [Test]
        public void Should_Ignore_Donor_That_Doesnt_Intersect_Inpaint_Area()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.StrictMock<IArea2DMapBuilder>();
            var imageArea = Area2D.Create(0, 0, 15, 15);

            var donor1 = Area2D.Create(0, 0, 2, 2);
            var inpaint = Area2D.Create(3, 3, 3, 3);

            mapBuilder.Expect(mb => mb.InitNewMap(imageArea, imageArea))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.SetIgnoredSourcedArea(Arg<Area2D>.Matches(x => x.IsSameAs(inpaint))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.Build())
                      .Return(null);

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            inpaintMapBuilder.InitNewMap(imageArea);
            inpaintMapBuilder.AddDonor(donor1);
            inpaintMapBuilder.SetInpaintArea(inpaint);
            inpaintMapBuilder.Build();

            mocks.VerifyAll();
        }

        [Test]
        public void Should_Ignore_Donor_That_Doesnt_Intersect_Last_Set_Inpaint_Area()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.StrictMock<IArea2DMapBuilder>();

            var imageArea = Area2D.Create(0, 0, 15, 15);

            var donor1 = Area2D.Create(0, 3, 8, 3);
            var inpaint1 = Area2D.Create(3, 3, 3, 3);
            var inpaint2 = Area2D.Create(0, 0, 2, 2);

            mapBuilder.Expect(mb => mb.InitNewMap(imageArea, imageArea))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.SetIgnoredSourcedArea(Arg<Area2D>.Matches(x => x.IsSameAs(inpaint1))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.SetIgnoredSourcedArea(Arg<Area2D>.Matches(x => x.IsSameAs(inpaint2))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.Build())
                      .Return(null);

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            inpaintMapBuilder.InitNewMap(imageArea);
            inpaintMapBuilder.AddDonor(donor1);
            inpaintMapBuilder.SetInpaintArea(inpaint1);
            inpaintMapBuilder.SetInpaintArea(inpaint2);
            inpaintMapBuilder.Build();

            mocks.VerifyAll();
        }

        [Test]
        public void Should_Ignore_Donor_That_Reside_Within_Inpaint_Area()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.StrictMock<IArea2DMapBuilder>();

            var imageArea = Area2D.Create(0, 0, 15, 15);

            var donor1 = Area2D.Create(3, 3, 2, 2);
            var inpaint1 = Area2D.Create(3, 3, 3, 3);

            mapBuilder.Expect(mb => mb.InitNewMap(imageArea, imageArea))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.SetIgnoredSourcedArea(Arg<Area2D>.Matches(x => x.IsSameAs(inpaint1))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.Build())
                      .Return(null);

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            inpaintMapBuilder.InitNewMap(imageArea);
            inpaintMapBuilder.AddDonor(donor1);
            inpaintMapBuilder.SetInpaintArea(inpaint1);
            inpaintMapBuilder.Build();

            mocks.VerifyAll();
        }


        [Test]
        public void Should_Ignore_Set_Donors_That_Were_Set_Before_Initialization()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.StrictMock<IArea2DMapBuilder>();

            var imageArea = Area2D.Create(0, 0, 15, 15);
            var donor1 = Area2D.Create(2, 3, 8, 3);
            var donor2 = Area2D.Create(0, 3, 8, 3);
            var inpaint = Area2D.Create(3, 3, 3, 3);

            var srcArea = Area2D.Create(0, 3, 3, 3).Join(Area2D.Create(6, 3, 2, 3));
            var dstArea = Area2D.Create(3, 3, 3, 3);

            mapBuilder.Expect(mb => mb.InitNewMap(imageArea, imageArea))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.InitNewMap(imageArea, imageArea))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.SetIgnoredSourcedArea(Arg<Area2D>.Matches(x => x.IsSameAs(inpaint))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.AddAssociatedAreas(Arg<Area2D>.Matches(x => x.IsSameAs(dstArea)),
                                                          Arg<Area2D>.Matches(x => x.IsSameAs(srcArea))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.Build())
                      .Return(null);

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            inpaintMapBuilder.InitNewMap(imageArea);
            inpaintMapBuilder.AddDonor(donor1);
            inpaintMapBuilder.InitNewMap(imageArea);
            inpaintMapBuilder.AddDonor(donor2);
            inpaintMapBuilder.SetInpaintArea(inpaint);
            inpaintMapBuilder.Build();

            mocks.VerifyAll();
        }

        [Test]
        public void Should_Add_Appropriate_Associated_Areas_For_Common_Parts_Of_Multiple_Donors()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.StrictMock<IArea2DMapBuilder>();

            var imageArea = Area2D.Create(0, 0, 10, 10);

            //     MARKUP                  DONOR1                  DONOR2
            //    0 1 2 3 4 5 6 7 8 9      0 1 2 3 4 5 6 7 8 9      0 1 2 3 4 5 6 7 8 9 
            // 0  0 0 0 0 0 0 0 0 0 0   0  1 1 1 1 1 1 0 0 0 0   0  0 0 0 0 0 0 0 0 0 0 
            // 1  0 0 0 0 0 0 0 0 0 0   1  1 1 1 1 1 1 0 0 0 0   1  0 0 0 0 0 0 0 0 0 0 
            // 2  0 0 1 1 1 1 1 1 0 0   2  1 1 1 1 1 1 0 0 0 0   2  0 0 0 0 0 0 0 0 0 0 
            // 3  0 0 1 1 1 1 1 1 0 0   3  1 1 1 1 1 1 0 0 0 0   3  0 0 0 0 0 0 0 0 0 0 
            // 4  0 0 1 1 1 1 1 1 0 0   4  1 1 1 1 1 1 0 0 0 0   4  1 1 1 1 1 1 0 0 0 0 
            // 5  0 0 1 1 1 1 1 1 0 0   5  1 1 1 1 1 1 0 0 0 0   5  1 1 1 1 1 1 0 0 0 0 
            // 6  0 0 1 1 1 1 1 1 0 0   6  0 0 0 0 0 0 0 0 0 0   6  1 1 1 1 1 1 0 0 0 0 
            // 7  0 0 1 1 1 1 1 1 0 0   7  0 0 0 0 0 0 0 0 0 0   7  1 1 1 1 1 1 0 0 0 0 
            // 8  0 0 0 0 0 0 0 0 0 0   8  0 0 0 0 0 0 0 0 0 0   8  1 1 1 1 1 1 0 0 0 0 
            // 9  0 0 0 0 0 0 0 0 0 0   9  0 0 0 0 0 0 0 0 0 0   9  1 1 1 1 1 1 0 0 0 0 

            var markup = Area2D.Create(2, 2, 6, 6);
            var donor1 = Area2D.Create(0, 0, 6, 6);
            var donor2 = Area2D.Create(0, 4, 6, 6);

            //    DONOR 3                  DONOR 4                  D1 & D2 & D3 & D4
            //    0 1 2 3 4 5 6 7 8 9      0 1 2 3 4 5 6 7 8 9      0 1 2 3 4 5 6 7 8 9 
            // 0  0 0 0 0 1 1 1 1 1 1   0  0 0 0 0 0 0 0 0 0 0   0  1 1 1 1 1 1 1 1 1 1 
            // 1  0 0 0 0 1 1 1 1 1 1   1  0 0 0 0 0 0 0 0 0 0   1  1 1 1 1 1 1 1 1 1 1 
            // 2  0 0 0 0 1 1 1 1 1 1   2  0 0 0 0 0 0 0 0 0 0   2  1 1 0 0 0 0 0 0 1 1 
            // 3  0 0 0 0 1 1 1 1 1 1   3  0 0 0 0 0 0 0 0 0 0   3  1 1 0 0 0 0 0 0 1 1 
            // 4  0 0 0 0 1 1 1 1 1 1   4  0 0 0 0 1 1 1 1 1 1   4  1 1 0 0 2 2 0 0 1 1 
            // 5  0 0 0 0 1 1 1 1 1 1   5  0 0 0 0 1 1 1 1 1 1   5  1 1 0 0 2 2 0 0 1 1 
            // 6  0 0 0 0 0 0 0 0 0 0   6  0 0 0 0 1 1 1 1 1 1   6  1 1 0 0 0 0 0 0 1 1 
            // 7  0 0 0 0 0 0 0 0 0 0   7  0 0 0 0 1 1 1 1 1 1   7  1 1 0 0 0 0 0 0 1 1 
            // 8  0 0 0 0 0 0 0 0 0 0   8  0 0 0 0 1 1 1 1 1 1   8  1 1 1 1 1 1 1 1 1 1 
            // 9  0 0 0 0 0 0 0 0 0 0   9  0 0 0 0 1 1 1 1 1 1   9  1 1 1 1 1 1 1 1 1 1 

            var donor3 = Area2D.Create(4, 0, 6, 6);
            var donor4 = Area2D.Create(4, 4, 6, 6);
            var d1d2d3d4_src = imageArea.Substract(markup);
            var d1d2d3d4_dest = Area2D.Create(4, 4, 2, 2);

            //    D1 & D2                   D1 & D3                 D1
            //    0 1 2 3 4 5 6 7 8 9      0 1 2 3 4 5 6 7 8 9      0 1 2 3 4 5 6 7 8 9 
            // 0  1 1 1 1 1 1 0 0 0 0   0  1 1 1 1 1 1 1 1 1 1   0  1 1 1 1 1 1 0 0 0 0 
            // 1  1 1 1 1 1 1 0 0 0 0   1  1 1 1 1 1 1 1 1 1 1   1  1 1 1 1 1 1 0 0 0 0 
            // 2  1 1 0 0 0 0 0 0 0 0   2  1 1 0 0 2 2 0 0 1 1   2  1 1 2 2 0 0 0 0 0 0 
            // 3  1 1 0 0 0 0 0 0 0 0   3  1 1 0 0 2 2 0 0 1 1   3  1 1 2 2 0 0 0 0 0 0 
            // 4  1 1 2 2 0 0 0 0 0 0   4  1 1 0 0 0 0 0 0 1 1   4  1 1 0 0 0 0 0 0 0 0 
            // 5  1 1 2 2 0 0 0 0 0 0   5  1 1 0 0 0 0 0 0 1 1   5  1 1 0 0 0 0 0 0 0 0 
            // 6  1 1 0 0 0 0 0 0 0 0   6  0 0 0 0 0 0 0 0 0 0   6  0 0 0 0 0 0 0 0 0 0 
            // 7  1 1 0 0 0 0 0 0 0 0   7  0 0 0 0 0 0 0 0 0 0   7  0 0 0 0 0 0 0 0 0 0 
            // 8  1 1 1 1 1 1 0 0 0 0   8  0 0 0 0 0 0 0 0 0 0   8  0 0 0 0 0 0 0 0 0 0 
            // 9  1 1 1 1 1 1 0 0 0 0   9  0 0 0 0 0 0 0 0 0 0   9  0 0 0 0 0 0 0 0 0 0 

            var d1d2_src = donor1.Join(donor2).Substract(markup);
            var d1d2_dest = Area2D.Create(2, 4, 2, 2);

            var d1d3_src = donor1.Join(donor3).Substract(markup);
            var d1d3_dest = Area2D.Create(4, 2, 2, 2);

            var d1_src = donor1.Substract(markup);
            var d1_dest = Area2D.Create(2, 2, 2, 2);

            //    D2                       D2 & D4                 D3 & D4
            //    0 1 2 3 4 5 6 7 8 9      0 1 2 3 4 5 6 7 8 9      0 1 2 3 4 5 6 7 8 9 
            // 0  0 0 0 0 0 0 0 0 0 0   0  0 0 0 0 0 0 0 0 0 0   0  0 0 0 0 1 1 1 1 1 1 
            // 1  0 0 0 0 0 0 0 0 0 0   1  0 0 0 0 0 0 0 0 0 0   1  0 0 0 0 1 1 1 1 1 1 
            // 2  0 0 0 0 0 0 0 0 0 0   2  0 0 0 0 0 0 0 0 0 0   2  0 0 0 0 0 0 0 0 1 1 
            // 3  0 0 0 0 0 0 0 0 0 0   3  0 0 0 0 0 0 0 0 0 0   3  0 0 0 0 0 0 0 0 1 1 
            // 4  1 1 0 0 0 0 0 0 0 0   4  1 1 0 0 0 0 0 0 1 1   4  0 0 0 0 0 0 2 2 1 1 
            // 5  1 1 0 0 0 0 0 0 0 0   5  1 1 0 0 0 0 0 0 1 1   5  0 0 0 0 0 0 2 2 1 1 
            // 6  1 1 2 2 0 0 0 0 0 0   6  1 1 0 0 2 2 0 0 1 1   6  0 0 0 0 0 0 0 0 1 1 
            // 7  1 1 2 2 0 0 0 0 0 0   7  1 1 0 0 2 2 0 0 1 1   7  0 0 0 0 0 0 0 0 1 1 
            // 8  1 1 1 1 1 1 0 0 0 0   8  1 1 1 1 1 1 1 1 1 1   8  0 0 0 0 1 1 1 1 1 1 
            // 9  1 1 1 1 1 1 0 0 0 0   9  1 1 1 1 1 1 1 1 1 1   9  0 0 0 0 1 1 1 1 1 1 

            var d2_src = donor2.Substract(markup);
            var d2_dest = Area2D.Create(2, 6, 2, 2);

            var d2d4_src = donor2.Join(donor4).Substract(markup);
            var d2d4_dest = Area2D.Create(4, 6, 2, 2);

            var d3d4_src = donor3.Join(donor4).Substract(markup);
            var d3d4_dest = Area2D.Create(6, 4, 2, 2);

            //    D3                       D4
            //    0 1 2 3 4 5 6 7 8 9      0 1 2 3 4 5 6 7 8 9   
            // 0  0 0 0 0 1 1 1 1 1 1   0  0 0 0 0 0 0 0 0 0 0   
            // 1  0 0 0 0 1 1 1 1 1 1   1  0 0 0 0 0 0 0 0 0 0   
            // 2  0 0 0 0 0 0 2 2 1 1   2  0 0 0 0 0 0 0 0 0 0   
            // 3  0 0 0 0 0 0 2 2 1 1   3  0 0 0 0 0 0 0 0 0 0   
            // 4  0 0 0 0 0 0 0 0 1 1   4  0 0 0 0 0 0 0 0 1 1   
            // 5  0 0 0 0 0 0 0 0 1 1   5  0 0 0 0 0 0 0 0 1 1   
            // 6  0 0 0 0 0 0 0 0 0 0   6  0 0 0 0 0 0 2 2 1 1   
            // 7  0 0 0 0 0 0 0 0 0 0   7  0 0 0 0 0 0 2 2 1 1   
            // 8  0 0 0 0 0 0 0 0 0 0   8  0 0 0 0 1 1 1 1 1 1   
            // 9  0 0 0 0 0 0 0 0 0 0   9  0 0 0 0 1 1 1 1 1 1    

            var d3_src = donor3.Substract(markup);
            var d3_dest = Area2D.Create(6, 2, 2, 2);

            var d4_src = donor4.Substract(markup);
            var d4_dest = Area2D.Create(6, 6, 2, 2);

            mapBuilder.Expect(mb => mb.InitNewMap(imageArea, imageArea))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.SetIgnoredSourcedArea(Arg<Area2D>.Matches(x => x.IsSameAs(markup))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.AddAssociatedAreas(Arg<Area2D>.Matches(x => x.IsSameAs(d1d2d3d4_dest)),
                                                          Arg<Area2D>.Matches(x => x.IsSameAs(d1d2d3d4_src))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.AddAssociatedAreas(Arg<Area2D>.Matches(x => x.IsSameAs(d1d2_dest)),
                                                          Arg<Area2D>.Matches(x => x.IsSameAs(d1d2_src))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.AddAssociatedAreas(Arg<Area2D>.Matches(x => x.IsSameAs(d1d3_dest)),
                                                          Arg<Area2D>.Matches(x => x.IsSameAs(d1d3_src))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.AddAssociatedAreas(Arg<Area2D>.Matches(x => x.IsSameAs(d2d4_dest)),
                                                          Arg<Area2D>.Matches(x => x.IsSameAs(d2d4_src))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.AddAssociatedAreas(Arg<Area2D>.Matches(x => x.IsSameAs(d3d4_dest)),
                                                          Arg<Area2D>.Matches(x => x.IsSameAs(d3d4_src))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.AddAssociatedAreas(Arg<Area2D>.Matches(x => x.IsSameAs(d1_dest)),
                                                          Arg<Area2D>.Matches(x => x.IsSameAs(d1_src))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.AddAssociatedAreas(Arg<Area2D>.Matches(x => x.IsSameAs(d2_dest)),
                                                          Arg<Area2D>.Matches(x => x.IsSameAs(d2_src))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.AddAssociatedAreas(Arg<Area2D>.Matches(x => x.IsSameAs(d3_dest)),
                                                          Arg<Area2D>.Matches(x => x.IsSameAs(d3_src))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.AddAssociatedAreas(Arg<Area2D>.Matches(x => x.IsSameAs(d4_dest)),
                                                          Arg<Area2D>.Matches(x => x.IsSameAs(d4_src))))
                      .Return(mapBuilder);
            mapBuilder.Expect(mb => mb.Build())
                      .Return(null);

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            inpaintMapBuilder.InitNewMap(imageArea);
            inpaintMapBuilder.AddDonor(donor1);
            inpaintMapBuilder.AddDonor(donor2);
            inpaintMapBuilder.AddDonor(donor3);
            inpaintMapBuilder.AddDonor(donor4);
            inpaintMapBuilder.SetInpaintArea(markup);
            inpaintMapBuilder.Build();

            mocks.VerifyAll();
        }

        [Test]
        [Ignore("Don't know yet how to handle this properly on Travis CI")]
        public void Should_Build_Proper_Mapping()
        {
            var testName = "InpaintMapBuilderTest";
            var ts = TestSet.Init("256x128");

            // create map 
            var mapBuilder = new InpaintMapBuilder();
            var imageArea = Area2D.Create(0, 0, ts.Picture.Width, ts.Picture.Height);
            mapBuilder.InitNewMap(imageArea);
            var inpaintArea = ts.RemoveMarkup.ToArea();
            mapBuilder.SetInpaintArea(inpaintArea);

            for (int i = 0; i < ts.Donors.Count; i++)
            {
                var donorArea = ts.Donors[i].ToArea();
                mapBuilder.AddDonor(donorArea);
            }

            var sw = new Stopwatch();
            sw.Restart();
            var mapping = mapBuilder.Build();
            sw.Stop();

            // convert mapping to areas
            var areaPairs = (mapping as IAreasMapping).AssociatedAreasAsc;
            for (int i = 0; i < areaPairs.Length; i++)
            {
                var areaPair = areaPairs[i];

                SaveToOutput(areaPair.Item1, $"dest{i}", testName, ts.Path, Color.Red);
                SaveToOutput(areaPair.Item2, $"src{i}", testName, ts.Path, Color.Green);
            }

            // compare output and references
            string[] reffiles = Directory.GetFiles($"{ts.Path}\\{testName}\\refs", "*.*", SearchOption.TopDirectoryOnly);
            string[] outfiles = Directory.GetFiles($"{ts.Path}\\{testName}\\output", "*.*", SearchOption.TopDirectoryOnly);

            Assert.That(reffiles.Length == outfiles.Length);

            if (reffiles.Length != outfiles.Length)

                foreach (var refFilePath in reffiles)
                {
                    var refFileName = Path.GetFileName(refFilePath);
                    var outFilePath = $"{ts.Path}\\{testName}\\output\\{refFileName}";

                    Assert.That(File.Exists(outFilePath));

                    var refArea = new Bitmap(refFilePath).ToArea();
                    var outArea = new Bitmap(outFilePath).ToArea();

                    Assert.That(refArea.IsSameAs(outArea));
                }
        }

        [Test]
        [Ignore("Don't know yet how to handle this properly on Travis CI")]
        public void Should_Build_Proper_Mapping_Fast()
        {
            var ts = TestSet.Init("1280x720");

            // create map 
            var mapBuilder = new InpaintMapBuilder();
            var imageArea = Area2D.Create(0, 0, ts.Picture.Width, ts.Picture.Height);
            mapBuilder.InitNewMap(imageArea);
            var inpaintArea = ts.RemoveMarkup.ToArea();
            mapBuilder.SetInpaintArea(inpaintArea);

            for (int i = 0; i < ts.Donors.Count; i++)
            {
                var donorArea = ts.Donors[i].ToArea();
                mapBuilder.AddDonor(donorArea);
            }

            var sw = new Stopwatch();
            sw.Restart();
            mapBuilder.Build();
            sw.Stop();

            Assert.That(sw.ElapsedMilliseconds < 800);
        }

        private static void SaveToOutput(Area2D area, string fileName, string testName, string testPath, Color color)
        {
            var bmp = area.ToBitmap(color);
            var dir = $"{testPath}\\{testName}\\output";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var path = $"{dir}\\{fileName}.png";
            bmp.Save(path, ImageFormat.Png);
        }
    }
}
