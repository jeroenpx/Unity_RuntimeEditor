using UnityEngine;

namespace Battlehub.Utils
{
    public static class MathHelper
    {
        public static float CountOfDigits(float number)
        {
            return (number == 0) ? 1.0f : Mathf.Ceil(Mathf.Log10(Mathf.Abs(number) + 0.5f));
        }

        public static bool Approximately(Vector3 a, Vector3 b, float epsilonSq = 0.01f * 0.01f)
        {
            return Vector3.SqrMagnitude(a - b) <= epsilonSq;
        }

        public static bool Approximately(Quaternion a, Quaternion b, float range = 1 - 0.99999998f)
        {
            return Quaternion.Dot(a, b) >= 1f - range;
        }

        public static bool RayIntersectsTriangle(Ray inRay, Vector3 inTriA, Vector3 inTriB, Vector3 inTriC, out float outDistance, out Vector3 outPoint)
        {
            outDistance = 0f;
            outPoint = Vector3.zero;

            //Find vectors for two edges sharing V1
            Vector3 e1 = inTriB - inTriA;
            Vector3 e2 = inTriC - inTriA;

            //Begin calculating determinant - also used to calculate `u` parameter
            Vector3 P = Vector3.Cross(inRay.direction, e2);

            //if determinant is near zero, ray lies in plane of triangle
            float det = Vector3.Dot(e1, P);

            if (det > -Mathf.Epsilon && det < Mathf.Epsilon)
            {
                return false;
            }

            float inv_det = 1f / det;

            //calculate distance from V1 to ray origin
            Vector3 T = inRay.origin - inTriA;

            // Calculate u parameter and test bound
            float u = Vector3.Dot(T, P) * inv_det;

            //The intersection lies outside of the triangle
            if (u < 0f || u > 1f)
            {
                return false;
            }
                
            //Prepare to test v parameter
            Vector3 Q = Vector3.Cross(T, e1);

            //Calculate V parameter and test bound
            float v = Vector3.Dot(inRay.direction, Q) * inv_det;

            //The intersection lies outside of the triangle
            if (v < 0f || u + v > 1f)
            {
                return false;
            }
                
            float t = Vector3.Dot(e2, Q) * inv_det;

            if (t > Mathf.Epsilon)
            {
                //ray intersection
                outDistance = t;

                outPoint.x = (u * inTriB.x + v * inTriC.x + (1 - (u + v)) * inTriA.x);
                outPoint.y = (u * inTriB.y + v * inTriC.y + (1 - (u + v)) * inTriA.y);
                outPoint.z = (u * inTriB.z + v * inTriC.z + (1 - (u + v)) * inTriA.z);

                return true;
            }

            return false;
        }
    }
}

