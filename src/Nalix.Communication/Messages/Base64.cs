using Nalix.Shared.LZ4;
using System.Text;

namespace Nalix.Communication.Messages;

public static class Base64
{
    public static System.String CompressToBase64(this System.String text)
    {
        // Với chuỗi ngắn, nén có thể không lợi — nhưng theo yêu cầu vẫn nén.
        if (System.String.IsNullOrEmpty(text))
        {
            return System.String.Empty;
        }

        System.ReadOnlySpan<System.Byte> input = Encoding.UTF8.GetBytes(text);
        System.Byte[] compressed = LZ4Codec.Encode(input); // trả về mảng đã cắt đúng 'written'
        return System.Convert.ToBase64String(compressed);
    }

    public static System.String DecompressFromBase64(this System.String base64)
    {
        if (System.String.IsNullOrEmpty(base64))
        {
            return System.String.Empty;
        }

        try
        {
            System.Byte[] compressed = System.Convert.FromBase64String(base64);

            // Dùng overload Decode(input, out output, out written)
            return !LZ4Codec.Decode(compressed, out System.Byte[] output, out System.Int32 written) ||
                output is null || written <= 0
                ? throw new System.InvalidOperationException("LZ4 decompression failed.")
                : Encoding.UTF8.GetString(output, 0, written);
        }
        catch (System.FormatException ex)
        {
            throw new System.InvalidOperationException("Invalid Base64 in credential field.", ex);
        }
    }
}
