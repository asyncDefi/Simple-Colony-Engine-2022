

namespace SType
{
    [System.Serializable]
    public class Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator UnityEngine.Vector2(Vector2 customVector)
        {
            return new UnityEngine.Vector2(customVector.x, customVector.y);
        }
        public static implicit operator Vector2(UnityEngine.Vector2 unityVector)
        {
            return new Vector2(unityVector.x, unityVector.y);
        }
    }

    [System.Serializable]
    public class Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator UnityEngine.Vector3(Vector3 customVector)
        {
            return new UnityEngine.Vector3(customVector.x, customVector.y, customVector.z);
        }

        public static implicit operator Vector3(UnityEngine.Vector3 unityVector)
        {
            return new Vector3(unityVector.x, unityVector.y, unityVector.z);
        }
    }

    [System.Serializable]
    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }


        public static implicit operator UnityEngine.Color(Color customColor)
        {
            return new UnityEngine.Color(customColor.r, customColor.g, customColor.b, customColor.a);
        }

        public static implicit operator Color(UnityEngine.Color unityColor)
        {
            return new Color(unityColor.r, unityColor.g, unityColor.b, unityColor.a);
        }
    }
}