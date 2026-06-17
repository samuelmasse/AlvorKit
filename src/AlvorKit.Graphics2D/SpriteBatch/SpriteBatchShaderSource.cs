namespace AlvorKit.Graphics2D;

/// <summary>Builds the GLSL source used by the default sprite batch shader.</summary>
internal static class SpriteBatchShaderSource
{
    /// <summary>Returns the default vertex shader source.</summary>
    internal static string Vert() =>
        """
        #version 330

        layout(location = 0) in vec2 inPosition;
        layout(location = 1) in vec4 inColor;
        layout(location = 2) in vec2 inTexCoord;
        layout(location = 3) in float inTexIndex;

        out vec4 fragColor;
        out vec2 fragTexCoord;
        flat out float fragTexIndex;

        void main() {
            fragColor = inColor;
            fragTexCoord = inTexCoord;
            fragTexIndex = inTexIndex;
            gl_Position = vec4(inPosition, -1, 1);
        }
        """;

    /// <summary>Returns fragment shader source that can sample from the requested texture slot count.</summary>
    internal static string Frag(int textureSlots)
    {
        var sb = new StringBuilder(
            $$"""
            #version 330

            in vec4 fragColor;
            in vec2 fragTexCoord;
            flat in float fragTexIndex;

            out vec4 outColor;

            uniform sampler2D texSamplers[{{textureSlots}}];

            void main() {
                int index = int(fragTexIndex);
                if (index == 0) outColor = texture(texSamplers[0], fragTexCoord) * fragColor;
                else if (index == 1) outColor = texture(texSamplers[1], fragTexCoord) * fragColor;

            """);

        for (var i = 2; i < textureSlots; i++)
            sb.AppendLine($"    else if (index == {i}) outColor = texture(texSamplers[{i}], fragTexCoord) * fragColor;");

        sb.AppendLine("}");
        return sb.ToString();
    }
}
