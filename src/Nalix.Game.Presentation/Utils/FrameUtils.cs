using SFML.Graphics;
using System.Collections.Generic;

namespace Nalix.Client.Desktop.Utils;

public static class FrameUtils
{
    // Phương thức tĩnh để tạo danh sách frames
    public static List<IntRect> GenerateFrames(int width, int height, int[] columnsPerRow)
    {
        List<IntRect> frames = [];
        int rows = columnsPerRow.Length; // Số dòng (tính theo số phần tử trong mảng columnsPerRow)

        // Duyệt qua từng dòng
        for (int row = 0; row < rows; row++)
        {
            int currentColumns = columnsPerRow[row]; // Số cột (frames) trong dòng hiện tại

            // Duyệt qua từng cột (frame) trong dòng
            for (int col = 0; col < currentColumns; col++)
            {
                // Tạo IntRect cho mỗi frame và thêm vào danh sách
                frames.Add(new IntRect(col * width, row * height, width, height));
            }
        }

        return frames;
    }
}