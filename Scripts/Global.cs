using System;
using System.Collections.Generic;
using Godot;

namespace ExodusGlobal
{
    public class Constants
    {
        // Shaders' parameters
        public const string DISTORTION_SHADER_SCREEN_DARKNESS = "screen_darkness";
        public const string DISTORTION_SHADER_DARKNESS_PROGRESSION = "darkness_progression";
        public const string DISTORTION_SHADER_UV_OFFSET = "uv_offset";
        public const string DISTORTION_SHADER_SIZE = "size";
        
        public const string VIGNETTE_SHADER_MULTIPLIER = "multiplier";
        public const string VIGNETTE_SHADER_SOFTNESS = "softness";
        
        public const string BLUR_SHADER_LIMIT = "limit";
        public const string BLUR_SHADER_BLUR = "blur";
        
        // Animation
        public const string PLAYERS_HEAD_ANIMATION_ON_DYING = "players_head_on_dying";

        // Math
        public const float ACCEPTABLE_TOLERANCE = 0.01f;
        
        // GODOT
        public const string RAYCAST_COLLIDER = "collider";
        public const string RAYCAST_POSITION = "position";
        public const string RAYCAST_NORMAL = "normal";
        
        // Surface types
        public const string Brick = "Brick";
        public const string Human = "Human";
        public const string Robot = "Robot";
        public const string Wood = "Wood";
        public const string Glass = "Glass";
        public const string Snow = "Snow";
        public const string Metal = "Metal";
        public const string Plastic = "Plastic";
        public const string Fabric = "Fabric";
        public const string Dirt = "Dirt";
    }
    
    public class CustomMath
    {
        public static float Lerp(float from, float to, float weight)
        {
            float result = Mathf.Lerp(
                from, to, weight);

            if (AreAlmostEqual(result, to)) return to;
            
            return result;
        }
        
        public static bool AreAlmostEqual(float a, float b)
        {
            float aAbs = Mathf.Abs(a);
            float bAbs = Mathf.Abs(b);
            
            float maxBetweenAbsolutes = Mathf.Max(aAbs, bAbs);
            
            return Mathf.Abs(a - b) <= Constants.ACCEPTABLE_TOLERANCE * Mathf.Max(1.0f, maxBetweenAbsolutes);
        }
    }

    public class Randomization
    {
        private static Random _random = new();
        
        public static int GetRandomIndex(int max)
        {
            // returns random number in the following range [0, max)
            return _random.Next(max);
        }

        public static T PickRandomItem<T>(List<T> items)
        {
            
            if (items == null || items.Count == 0)
                throw new ArgumentException("The list cannot be null or empty.");
            
            // // Get a random index using RandomNumberGenerator
            int randomIndex = _random.Next(items.Count);
            //
            // // Return the item at the random index
            return items[randomIndex];
        }
    }

    public class Surface
    {
        public static string GetSurfaceOrigin(Node target)
        {
            
        if (target.IsInGroup(Constants.Human))
        {
            return Constants.Human;
        }
        
        if (target.IsInGroup(Constants.Robot))
        {
            return Constants.Robot;
        }
        
        if (target.IsInGroup(Constants.Wood))
        {
            return Constants.Wood;
        }
        
        if (target.IsInGroup(Constants.Glass))
        {
            return Constants.Glass;
        }
        
        if (target.IsInGroup(Constants.Snow))
        {
            return Constants.Snow;
        }
        
        if (target.IsInGroup(Constants.Metal))
        {
            return Constants.Metal;
        }
        
        if (target.IsInGroup(Constants.Plastic))
        {
            return Constants.Plastic;
        }
        
        if (target.IsInGroup(Constants.Fabric))
        {
            return Constants.Fabric;
        }
        
        if (target.IsInGroup(Constants.Dirt))
        {
            return Constants.Dirt;
        }

        return Constants.Metal;
        }
    }
}

