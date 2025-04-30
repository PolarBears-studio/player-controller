using Godot;

namespace PolarBears.PlayerControllerAddon;

public class Constants
{
    // Shaders' parameters
    public const string DISTORTION_SHADER_SCREEN_DARKNESS      = "screen_darkness";
    public const string DISTORTION_SHADER_DARKNESS_PROGRESSION = "darkness_progression";
    public const string DISTORTION_SHADER_UV_OFFSET            = "uv_offset";
    public const string DISTORTION_SHADER_SIZE                 = "size";

    public const string VIGNETTE_SHADER_MULTIPLIER = "multiplier";
    public const string VIGNETTE_SHADER_SOFTNESS   = "softness";

    public const string BLUR_SHADER_LIMIT = "limit";
    public const string BLUR_SHADER_BLUR  = "blur";

    // Animation
    public const string PLAYERS_HEAD_ANIMATION_ON_DYING = "players_head_on_dying";

    // Math
    public const float ACCEPTABLE_TOLERANCE = 0.01f;
}

