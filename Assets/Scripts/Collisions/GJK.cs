using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Schema;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms.Impl;
using Debug = UnityEngine.Debug;

namespace Collisions {
    public class GJK : MonoBehaviour {

        public static bool Has3Dimensions = false;
        
        public static Vector3 Support(Vector3 direction, Vector3[] pointsA, Vector3[] pointsB) {
            return Support(direction, pointsA) - Support(-direction, pointsB);
        }

        public static Vector3 Support(Vector3 direction, Vector3[] points) {

            Vector3 furthestPoint = points[0];
            float furthestDotProduct = Vector3.Dot(direction, furthestPoint);
            
            for (int i = 1; i < points.Length; i++) {
                
                float dot = Vector3.Dot(direction, points[i]);

                if (furthestDotProduct < dot) {
                    furthestPoint = points[i];
                    furthestDotProduct = dot;
                }
                
            }

            return furthestPoint;
        }
        
        public static bool ContainsOrigin(ref Vector3 direction, ref List<Vector3> points) {
            if (!Has3Dimensions) {
                return ContainsOrigin2D(ref direction, ref points);
            } else {
                return ContainsOrigin3D(ref direction, ref points);
            }
        }
        public static bool ContainsOrigin3D(ref Vector3 direction, ref List<Vector3> points) {
            Vector3 a, b, c, d, ab, a0, ac, ad, abc, acd, abd;
            switch (points.Count) {
                case 4:
                    a = points[0];
                    b = points[1];
                    c = points[2];
                    d = points[3];

                    ab = b - a;
                    ac = c - a;
                    ad = d - a;
                    a0 = -a;
                    
                    abc = Vector3.Cross(ab, ac);
                    acd = Vector3.Cross(ac, ad);
                    abd = Vector3.Cross(ad, ab);
                    
                    if (Vector3.Dot(abc, a0) > 0) {
                        points.RemoveAt(3);
                        return Triangle(ref points, ref direction);
                    }else if (Vector3.Dot(acd, a0) > 0) {
                        points.RemoveAt(1);
                        return Triangle(ref points, ref direction);
                    }else if (Vector3.Dot(abd, a0) > 0) {
                        points.Clear();
                        points.Add(a);
                        points.Add(d);
                        points.Add(b);
                        return Triangle(ref points, ref direction);
                    } else {
                        return true;
                    }
                    
                    return false;
                case 3:


                    a = points[0];
                    b = points[1];
                    c = points[2];

                    ab = b - a;
                    ac = c - a;
                    a0 = -a;

                    abc = Vector3.Cross(ab, ac);

                    if (Vector3.Dot(Vector3.Cross(abc, ac), a0) > 0) {
                        
                        if (Vector3.Dot(ac, a0) > 0) {
                            
                            points.RemoveAt(1);
                            direction = Vector3.Cross(Vector3.Cross(ac, a0), ac);
                            
                        } else {
                            
                            points.RemoveAt(2);
                            
                        }
                        
                    } else {
                        
                        if (Vector3.Dot(Vector3.Cross(ab, abc), a0) > 0) {
                            
                            points.RemoveAt(2);
                            
                        } else {
                            
                            if (Vector3.Dot(abc, a0) > 0) {
                                direction.Set(abc.x, abc.y, abc.z);
                            } else {
                                points.Clear();
                                points.Add(a);
                                points.Add(c);
                                points.Add(b);
                                abc = -abc;
                                direction.Set(abc.x, abc.y, abc.z);
                            }
                            
                        }
                    }
                    
                    
                    return false;
                case 2:

                    return Line(ref points, ref direction);
            }

            return false;
        }

        private static bool Line(ref List<Vector3> points, ref Vector3 direction) {
            Vector3 a = points[0];
            Vector3 b = points[1];

            Vector3 ab = b - a;
            Vector3 a0 = -a;

            if (Vector3.Dot(ab, a0) > 0) {
                direction = Vector3.Cross(Vector3.Cross(ab, a0), ab);
            } else {
                points.RemoveAt(1);
                direction = a0;
            }
                    
            return false;
        }
        
        private static bool Triangle(ref List<Vector3> points, ref Vector3 direction) {
            Vector3 a = points[0];
            Vector3 b = points[1];
            Vector3 c = points[2];

            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 a0 = -a;

            Vector3 abc = Vector3.Cross(ab, ac);

            if (Vector3.Dot(Vector3.Cross(abc, ac), a0) > 0) {
                        
                if (Vector3.Dot(ac, a0) > 0) {
                            
                    points.RemoveAt(1);
                    direction = Vector3.Cross(Vector3.Cross(ac, a0), ac);
                } else {
                            
                    points.RemoveAt(2);
                            
                    return Line(ref points, ref direction);
                }
                        
            } else {
                        
                if (Vector3.Dot(Vector3.Cross(ab, abc), a0) > 0) {
                            
                    points.RemoveAt(2);
                    return Line(ref points, ref direction);
                            
                } else {
                            
                    if (Vector3.Dot(abc, a0) > 0) {
                        direction = abc;
                    } else {
                        points.Clear();
                        points.Add(a);
                        points.Add(c);
                        points.Add(b);
                        direction = -abc;
                    }
                            
                }
            }
                    
                    
            return false;
        }
        
        public static bool ContainsOrigin2D(ref Vector3 direction, ref List<Vector3> points) {

            
            Vector3 a = points[points.Count - 1];
            Vector3 a0 = -a;

             if (points.Count == 3) {
                Vector3 b = points[1];
                Vector3 c = points[0];

                Vector3 ab = b - a;
                Vector3 ac = c - a;
                
                Vector3 abPerp = Vector3.Cross(Vector3.Cross(ac, ab), ab);

                if (Vector3.Dot(abPerp, a0) > 0) {
                    
                    points.RemoveAt(0);

                    direction.Set(abPerp.x, abPerp.y, abPerp.z);
                    
                } else {
                    
                    Vector3 acPerp = Vector3.Cross(Vector3.Cross(ab, ac), ac);

                    if (Vector3.Dot(acPerp, a0) > 0) {
                        points.RemoveAt(1);
                        
                        direction.Set(acPerp.x, acPerp.y, acPerp.z);
                    } else {
                        return true;
                    }
                    
                }
                
            } else {
                Vector3 b = points[0];

                Vector3 ab = b - a;

                Vector3 abPerp = Vector3.Cross(Vector3.Cross(ab, a0), ab);
                
                direction.Set(abPerp.x, abPerp.y, abPerp.z);
            }
            
            
            return false;
        }
    
        
        
        public static bool Execute(Vector3[] pointsA, Vector3[] pointsB, [CanBeNull] out List<Vector3> LastSimplex) {
            
            List<Vector3> points = new List<Vector3>();
            
            Vector3 direction = Vector3.right; //Pick a direction to start off with (For now its random, probably should select an actual good point)
            
            points.Add(Support(direction, pointsA, pointsB) );

            direction = -points[0];

            int iterationAmount = 0;
            
            while (true) {
                iterationAmount++;

                Vector3 somePoint = Support(direction, pointsA, pointsB);
                
                if (Vector3.Dot(somePoint, direction) <= 0) {
                    //if(iterationAmount > 1)Debug.Log("Failed: " + iterationAmount + " | " + points.Count + " " + direction);
                    LastSimplex = points;

                    return false;
                }
                
                if(iterationAmount > 1000) {
                    Debug.Log("Failed");
                    LastSimplex = points;
                    return false;
                }
                
                if(Has3Dimensions) {
                    points.Insert(0, somePoint);
                }else {
                    points.Add(somePoint);
                }

                if (ContainsOrigin(ref direction, ref points)) {
                    LastSimplex = points;
                    return true;
                }
            }
        }
        
    }
}
