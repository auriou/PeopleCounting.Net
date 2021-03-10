//namespace Namespace
//{

//    using dist = scipy.spatial.distance;

//    using OrderedDict = collections.OrderedDict;

//    using np = numpy;

//    using System.Linq;

//    using System;

//    using System.Collections.Generic;

//    public static class Module
//    {

//        public class CentroidTracker
//        {
//            private int nextObjectID;
//            private List<int> objects;
//            private List<int> disappeared;
//            private int maxDisappeared;
//            private int maxDistance;

//            public CentroidTracker(int maxDisappeared = 50, int maxDistance = 50)
//            {
//                // initialize the next unique object ID along with two ordered
//                // dictionaries used to keep track of mapping a given object
//                // ID to its centroid and number of consecutive frames it has
//                // been marked as "disappeared", respectively
//                this.nextObjectID = 0;
//                this.objects = new List<int>();
//                this.disappeared = new List<int>();
//                // store the number of maximum consecutive frames a given
//                // object is allowed to be marked as "disappeared" until we
//                // need to deregister the object from tracking
//                this.maxDisappeared = maxDisappeared;
//                // store the maximum distance between centroids to associate
//                // an object -- if the distance is larger than this maximum
//                // distance we'll start to mark the object as "disappeared"
//                this.maxDistance = maxDistance;
//            }

//            public virtual object Register(object centroid)
//            {
//                // when registering an object we use the next available object
//                // ID to store the centroid
//                this.objects[this.nextObjectID] = centroid;
//                this.disappeared[this.nextObjectID] = 0;
//                this.nextObjectID += 1;
//            }

//            public virtual object deregister(object objectID)
//            {
//                // to deregister an object ID we delete the object ID from
//                // both of our respective dictionaries
//                this.objects.Remove(objectID);
//                this.disappeared.Remove(objectID);
//            }

//            public virtual object update(object rects)
//            {
//                object objectID;
//                // check to see if the list of input bounding box rectangles
//                // is empty
//                if (rects.Count == 0)
//                {
//                    // loop over any existing tracked objects and mark them
//                    // as disappeared
//                    foreach (var objectID in this.disappeared.keys().ToList())
//                    {
//                        this.disappeared[objectID] += 1;
//                        // if we have reached a maximum number of consecutive
//                        // frames where a given object has been marked as
//                        // missing, deregister it
//                        if (this.disappeared[objectID] > this.maxDisappeared)
//                        {
//                            this.deregister(objectID);
//                        }
//                    }
//                    // return early as there are no centroids or tracking info
//                    // to update
//                    return this.objects;
//                }
//                // initialize an array of input centroids for the current frame
//                var inputCentroids = np.zeros((rects.Count, 2), dtype: "int");
//                // loop over the bounding box rectangles
//                foreach (var _tup_1 in rects.Select((_p_1, _p_2) => Tuple.Create(_p_2, _p_1)))
//                {
//                    var i = _tup_1.Item1;
//                    (startX, startY, endX, endY) = _tup_1.Item2;
//                    // use the bounding box coordinates to derive the centroid
//                    var cX = Convert.ToInt32((startX + endX) / 2.0);
//                    var cY = Convert.ToInt32((startY + endY) / 2.0);
//                    inputCentroids[i] = (cX, cY);
//                }
//                // if we are currently not tracking any objects take the input
//                // centroids and register each of them
//                if (this.objects.Count == 0)
//                {
//                    foreach (var i in Enumerable.Range(0, inputCentroids.Count - 0))
//                    {
//                        this.register(inputCentroids[i]);
//                    }
//                }
//                else
//                {
//                    // otherwise, are are currently tracking objects so we need to
//                    // try to match the input centroids to existing object
//                    // centroids
//                    // grab the set of object IDs and corresponding centroids
//                    var objectIDs = this.objects.keys().ToList();
//                    var objectCentroids = this.objects.values().ToList();
//                    // compute the distance between each pair of object
//                    // centroids and input centroids, respectively -- our
//                    // goal will be to match an input centroid to an existing
//                    // object centroid
//                    var D = dist.cdist(np.array(objectCentroids), inputCentroids);
//                    // in order to perform this matching we must (1) find the
//                    // smallest value in each row and then (2) sort the row
//                    // indexes based on their minimum values so that the row
//                    // with the smallest value as at the *front* of the index
//                    // list
//                    var rows = D.min(axis: 1).argsort();
//                    // next, we perform a similar process on the columns by
//                    // finding the smallest value in each column and then
//                    // sorting using the previously computed row index list
//                    var cols = D.argmin(axis: 1)[rows];
//                    // in order to determine if we need to update, register,
//                    // or deregister an object we need to keep track of which
//                    // of the rows and column indexes we have already examined
//                    var usedRows = new HashSet<object>();
//                    var usedCols = new HashSet<object>();
//                    // loop over the combination of the (row, column) index
//                    // tuples
//                    foreach (var _tup_2 in zip(rows, cols))
//                    {
//                        var row = _tup_2.Item1;
//                        var col = _tup_2.Item2;
//                        // if we have already examined either the row or
//                        // column value before, ignore it
//                        if (usedRows.Contains(row) || usedCols.Contains(col))
//                        {
//                            continue;
//                        }
//                        // if the distance between centroids is greater than
//                        // the maximum distance, do not associate the two
//                        // centroids to the same object
//                        if (D[row, col] > this.maxDistance)
//                        {
//                            continue;
//                        }
//                        // otherwise, grab the object ID for the current row,
//                        // set its new centroid, and reset the disappeared
//                        // counter
//                        objectID = objectIDs[row];
//                        this.objects[objectID] = inputCentroids[col];
//                        this.disappeared[objectID] = 0;
//                        // indicate that we have examined each of the row and
//                        // column indexes, respectively
//                        usedRows.add(row);
//                        usedCols.add(col);
//                    }
//                    // compute both the row and column index we have NOT yet
//                    // examined
//                    var unusedRows = new HashSet<object>(Enumerable.Range(0, D.shape[0] - 0)).difference(usedRows);
//                    var unusedCols = new HashSet<object>(Enumerable.Range(0, D.shape[1] - 0)).difference(usedCols);
//                    // in the event that the number of object centroids is
//                    // equal or greater than the number of input centroids
//                    // we need to check and see if some of these objects have
//                    // potentially disappeared
//                    if (D.shape[0] >= D.shape[1])
//                    {
//                        // loop over the unused row indexes
//                        foreach (var row in unusedRows)
//                        {
//                            // grab the object ID for the corresponding row
//                            // index and increment the disappeared counter
//                            objectID = objectIDs[row];
//                            this.disappeared[objectID] += 1;
//                            // check to see if the number of consecutive
//                            // frames the object has been marked "disappeared"
//                            // for warrants deregistering the object
//                            if (this.disappeared[objectID] > this.maxDisappeared)
//                            {
//                                this.deregister(objectID);
//                            }
//                        }
//                    }
//                    else
//                    {
//                        // otherwise, if the number of input centroids is greater
//                        // than the number of existing object centroids we need to
//                        // register each new input centroid as a trackable object
//                        foreach (var col in unusedCols)
//                        {
//                            this.register(inputCentroids[col]);
//                        }
//                    }
//                }
//                // return the set of trackable objects
//                return this.objects;
//            }
//        }
//    }
//}