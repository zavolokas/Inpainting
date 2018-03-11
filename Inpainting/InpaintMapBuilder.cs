using System;
using System.Collections.Generic;
using System.Linq;
using Zavolokas.Math.Combinatorics;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting
{
    internal class InpaintMapBuilder
    {
        private readonly IArea2DMapBuilder _mapBuilder;
        private readonly IList<Area2D> _donors;
        private bool _isInitialized = false;
        private Area2D _sourceArea;
        private Area2D _inpaintArea;

        public InpaintMapBuilder()
            :this(new Area2DMapBuilder())
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="InpaintMapBuilder" /> class.
        /// </summary>
        /// <param name="mapBuilder">The map builder.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        /// <exception cref="System.NotImplementedException"></exception>
        public InpaintMapBuilder(IArea2DMapBuilder mapBuilder)
        {
            if (mapBuilder == null)
                throw new ArgumentNullException(nameof(mapBuilder));

            _mapBuilder = mapBuilder;
            _donors = new List<Area2D>();
        }

        /// <summary>
        /// Initializes the new map.
        /// </summary>
        /// <param name="mapping">The mapping.</param>
        /// <remarks>
        /// - Requires not null IPointToAreaMapping.
        /// - Requires that mapping has a number of dest points.
        /// - Requires that at least one dest point points to not empty source area.
        /// - Discards all the previous settings made by other methods.
        /// </remarks>
        public InpaintMapBuilder InitNewMap(IAreasMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            _sourceArea = mapping.AssociatedAreasAsc.Aggregate(Area2D.Empty, (current, area) => current.Join(area.Item2));

            _mapBuilder.InitNewMap(mapping);
            _isInitialized = true;
            _donors.Clear();
            _inpaintArea = null;
            return this;
        }

        /// <summary>
        /// Initializes a new map.
        /// </summary>
        /// <param name="imageArea">The area of an image.</param>
        /// <remarks>
        /// - Dest area can not be empty.
        /// - Source area can not be empty.
        /// - Discards all the previous settings made by other methods.
        /// </remarks>
        public InpaintMapBuilder InitNewMap(Area2D imageArea)
        {
            if (imageArea == null)
                throw new ArgumentNullException();

            if (imageArea.IsEmpty)
                throw new EmptyAreaException();

            _sourceArea = imageArea;
            _mapBuilder.InitNewMap(imageArea, imageArea);
            _isInitialized = true;
            _donors.Clear();
            _inpaintArea = null;

            return this;
        }

        /// <summary>
        /// Reduces the dest area.
        /// </summary>
        /// <param name="reducedArea">The reduced area.</param>
        /// <remarks>
        /// - Can not be executed before InitNewMap method.
        /// - Requires that the reducedArea is not empty.
        /// - Requires that reduced area in intersection with dest area not results in empty area.
        /// - Multiple call of the method has a cumulutive effect - result dest area should be an intersection of all passed areas and the dest
        /// </remarks>
        public InpaintMapBuilder ReduceDestArea(Area2D reducedArea)
        {
            if (reducedArea == null)
                throw new ArgumentNullException(nameof(reducedArea));

            if (reducedArea.IsEmpty)
                throw new EmptyAreaException();

            if (!_isInitialized)
                throw new MapIsNotInitializedException();

            _mapBuilder.ReduceDestArea(reducedArea);

            return this;
        }

        /// <summary>
        /// Sets the area that should be inpainted.
        /// </summary>
        /// <param name="inpaintArea">The inpaint area.</param>
        /// <remarks>
        /// - Can not be executed before InitNewMap method.
        /// - Requires that the inpaintArea is not empty.
        /// - Requires that the inpaintArea not equal or include whole image area.
        /// </remarks>
        public InpaintMapBuilder SetInpaintArea(Area2D inpaintArea)
        {
            if (inpaintArea == null)
                throw new ArgumentNullException(nameof(inpaintArea));

            if (inpaintArea.IsEmpty)
                throw new EmptyAreaException();

            if (!_isInitialized)
                throw new MapIsNotInitializedException();

            _inpaintArea = _sourceArea.Intersect(inpaintArea);
            _mapBuilder.SetIgnoredSourcedArea(_inpaintArea);
            return this;
        }

        /// <summary>
        /// Builds the mapping.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// - Can not be executed before InitNewMap method.
        /// - Can not be executed before SetInpaintArea method;
        /// </remarks>
        public Area2DMap Build()
        {
            if (!_isInitialized)
                throw new MapIsNotInitializedException();

            if (_inpaintArea == null)
                throw new InpaintAreaIsNotSetException();

            if (_donors != null && _donors.Count > 0)
            {
                var actualDonors = new List<Area2D>();

                //collect only donors that have an intersection with the inpaint area
                foreach (var donor in _donors)
                {
                    if (donor.Intersect(_inpaintArea).IsEmpty || donor.Substract(_inpaintArea).IsEmpty)
                        continue;

                    actualDonors.Add(donor.Intersect(_sourceArea));
                }

                Tuple<Area2D, Area2D>[] destSourceAreaPairs = ExtractDestSourceAreaPairs(actualDonors, _inpaintArea);

                foreach (var destSourceAreaPair in destSourceAreaPairs)
                {
                    _mapBuilder.AddAssociatedAreas(destSourceAreaPair.Item1, destSourceAreaPair.Item2);
                }
            }

            var result = _mapBuilder.Build();
            return result;
        }

        private Tuple<Area2D, Area2D>[] ExtractDestSourceAreaPairs(List<Area2D> donors, Area2D inpaintArea)
        {
            // to some destination areas within the inpaint area 
            // can relate many source areas. In order to find such 
            // relations we need to check all the combinations of the donors.
            // Since we can assiciate a dest area only once, we are
            // interested in checking the longest area combination first.
            var combinations = donors.GetAllCombinations()
                .OrderByDescending(x=>x.Count());
                //.ToList();
                
            var donoredArea = Area2D.Empty;

            return combinations.Select(combination => ToDestSourceAreaPair(combination, inpaintArea, ref donoredArea))
                .Where(destSourceAreaPair => destSourceAreaPair != null)
                .ToArray();
        }

        private Tuple<Area2D, Area2D> ToDestSourceAreaPair(IEnumerable<Area2D> donors, Area2D inpaintArea, ref Area2D donoredDestArea)
        {
            bool allIntersect = true;
            var donorArray = donors as Area2D[] ?? donors.ToArray();

            // We will try to create a pair of associated dest and source areas
            // based on provided donor areas and area to inpaint.
            // The common part of the donors that reside within the inpaint area is a new dest area.
            // The parts of the donors that are outside of the inpaint area is a new source area.
            var destArea = donorArray.First();
            var srcArea = Area2D.Empty;

            foreach (var donor in donorArray)
            {
                // get the donor dest part (the part that reside within the inpaint area)
                var donorDest = donor.Intersect(inpaintArea);

                // construct the intersection of all the donors dests
                destArea = destArea.Intersect(donorDest);
                if (!destArea.IsEmpty)
                {
                    // Construct the source area for the common parts of the donor dests.
                    // This is a conjunction of their source parts (the parts that are reside outside of the inpaint areas)
                    srcArea = srcArea.Join(donor.Substract(inpaintArea));
                }
                else
                {
                    // not all the donors in the combination have a common dest part
                    // that is why it is not succeed to create the association.
                    allIntersect = false;
                    break;
                }
            }

            if (allIntersect)
            {
                // All the dest parts of donors has not empty intersection.
                // Exlude the part of the donor destination that was already donored.
                destArea = destArea.Substract(donoredDestArea);
                if (!destArea.IsEmpty)
                {
                    // It is still not empty.
                    // Mark extra part of the dest area as donored
                    donoredDestArea = donoredDestArea.Join(destArea);

                    // and return associated dest and source pair
                    return new Tuple<Area2D, Area2D>(destArea, srcArea);
                }
            }
            return null;
        }

        /// <summary>
        /// Adds the donor.
        /// </summary>
        /// <param name="donorArea">The donor area.</param>
        /// <remarks>
        /// - Can not be called before Initialization.
        /// - Donor area can not be NULL.
        /// - Donor area can not be Empty.
        /// </remarks>
        public InpaintMapBuilder AddDonor(Area2D donorArea)
        {
            if (donorArea == null)
                throw new ArgumentNullException(nameof(donorArea));

            if (donorArea.IsEmpty)
                throw new EmptyAreaException();

            if (!_isInitialized)
                throw new MapIsNotInitializedException();

            _donors.Add(donorArea);

            return this;
        }
    }
}
