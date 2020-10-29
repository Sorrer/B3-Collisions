using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Collisions {
    public class GJK : MonoBehaviour
    {
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

            
            Vector3 a = points[points.Count - 1];
            Vector3 a0 = -a;

            if (points.Count == 4) {
                
            }else if (points.Count == 3) {
                Vector3 b = points[1];
                Vector3 c = points[0];

                Vector3 ab = b - a;
                Vector3 ac = c - a;
                
                Vector3 abPerp = Vector3.Cross(Vector3.Cross(ac, ab), ab);
                Vector3 acPerp = Vector3.Cross(Vector3.Cross(ab, ac), ac);

                if (Vector3.Dot(abPerp, a0) > 0) {
                    
                    points.RemoveAt(0);
                    
                    direction.Set(abPerp.x, abPerp.y, abPerp.z);
                    
                } else {

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

        public static bool Execute(Vector3[] pointsA, Vector3[] pointsB) {
            List<Vector3> points = new List<Vector3>();
            
            Vector3 Direction = Vector3.right; //Pick a direction to start off with (For now its random, probably should select an actual good point)
            
            points.Add(Support(Direction, pointsA, pointsB) );

            Direction = -points[0];

            int iterationAmount = 0;
            
            while (true) {
                iterationAmount++;
                Vector3 somePoint = Support(Direction, pointsA, pointsB);

                if (Vector3.Dot(somePoint, Direction) <= 0) {
                    //Debug.Log("Failed: " + iterationAmount);
                    return false;
                }
                
                points.Add(somePoint);


                if (ContainsOrigin(ref Direction, ref points)) {
                    return true;
                }
            }
        }
        
    }
}
