// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using Windows.UI.Xaml.Media;

namespace Microsoft.Toolkit.Uwp.UI.Extensions
{
    /// <summary>
    /// Provides a set of extensions to the <see cref="o:Windows.UI.Xaml.Media.Matrix"/> struct.
    /// </summary>
    public static class MatrixExtensions
    {
        /// <summary>
        /// Implements WPF's Matrix.HasInverse.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <returns>True if matrix has an inverse.</returns>
        public static bool HasInverse(this Matrix matrix)
        {
            // TODO: Check if we can make this an extension property in C#8.

            // WPF equivalent of following code:
            // return matrix.HasInverse;
            return ((matrix.M11 * matrix.M22) - (matrix.M12 * matrix.M21)) != 0;
        }

        /// <summary>
        /// In-place multiply this matrix to the given matrix.
        /// </summary>
        /// <param name="matrix1">Initial matrix.</param>
        /// <param name="matrix2">Matrix to multiply by.</param>
        public static void Multiply(this ref Matrix matrix1, Matrix matrix2)
        {
            var temp = MatrixHelperEx.Multiply(matrix1, matrix2);

            matrix1.M11 = temp.M11;
            matrix1.M12 = temp.M12;
            matrix1.M21 = temp.M21;
            matrix1.M22 = temp.M22;
            matrix1.OffsetX = temp.OffsetX;
            matrix1.OffsetY = temp.OffsetY;
        }

        /// <summary>
        /// Applies a rotation of the specified angle about the origin of this Matrix structure.
        /// </summary>
        /// <param name="matrix">Matrix to extend.</param>
        /// <param name="angle">The angle of rotation in degrees.</param>
        public static void Rotate(this ref Matrix matrix, double angle)
        {
            matrix.Multiply(CreateRotationRadians((angle % 360) * (Math.PI / 180.0)));
        }

        /// <summary>
        /// Rotates this matrix about the specified point.
        /// </summary>
        /// <param name="matrix">Matrix to extend.</param>
        /// <param name="angle">The angle of rotation in degrees.</param>
        /// <param name="centerX">The x-coordinate of the point about which to rotate this matrix.</param>
        /// <param name="centerY">The y-coordinate of the point about which to rotate this matrix.</param>
        public static void RotateAt(this ref Matrix matrix, double angle, double centerX, double centerY)
        {
            matrix.Multiply(CreateRotationRadians((angle % 360) * (Math.PI / 180.0), centerX, centerY));
        }

        /// <summary>
        /// Appends the specified scale vector to this Matrix structure.
        /// </summary>
        /// <param name="matrix">Matrix to extend.</param>
        /// <param name="scaleX">The value by which to scale this Matrix along the x-axis.</param>
        /// <param name="scaleY">The value by which to scale this Matrix along the y-axis.</param>
        public static void Scale(this ref Matrix matrix, double scaleX, double scaleY)
        {
            matrix.Multiply(CreateScaling(scaleX, scaleY));
        }

        /// <summary>
        /// Scales this Matrix by the specified amount about the specified point.
        /// </summary>
        /// <param name="matrix">Matrix to extend.</param>
        /// <param name="scaleX">The value by which to scale this Matrix along the x-axis.</param>
        /// <param name="scaleY">The value by which to scale this Matrix along the y-axis.</param>
        /// <param name="centerX">The x-coordinate of the scale operation's center point.</param>
        /// <param name="centerY">The y-coordinate of the scale operation's center point.</param>
        public static void ScaleAt(this ref Matrix matrix, double scaleX, double scaleY, double centerX, double centerY)
        {
            matrix.Multiply(CreateScaling(scaleX, scaleY, centerX, centerY));
        }

        /// <summary>
        /// Appends a skew of the specified degrees in the x and y dimensions to this Matrix structure.
        /// </summary>
        /// <param name="matrix">Matrix to extend.</param>
        /// <param name="skewX">The angle in the x dimension by which to skew this Matrix.</param>
        /// <param name="skewY">The angle in the y dimension by which to skew this Matrix.</param>
        public static void Skew(this ref Matrix matrix, double skewX, double skewY)
        {
            matrix.Multiply(CreateSkewRadians((skewX % 360) * (Math.PI / 180.0), (skewY % 360) * (Math.PI / 180.0)));
        }

        /// <summary>
        /// Translates this matrix.
        /// </summary>
        /// <param name="matrix">Matrix to extend.</param>
        /// <param name="offsetX">The offset in the x dimension.</param>
        /// <param name="offsetY">The offset in the y dimension.</param>
        public static void Translate(this ref Matrix matrix, double offsetX, double offsetY)
        {
            matrix.OffsetX += offsetX;
            matrix.OffsetY += offsetY;
        }

        internal static Matrix CreateRotationRadians(double angle)
        {
            return CreateRotationRadians(angle, 0, 0);
        }

        internal static Matrix CreateRotationRadians(double angle, double centerX, double centerY)
        {
            var sin = Math.Sin(angle);
            var cos = Math.Cos(angle);
            var dx = (centerX * (1.0 - cos)) + (centerY * sin);
            var dy = (centerY * (1.0 - cos)) - (centerX * sin);

            #pragma warning disable SA1117 // Parameters must be on same line or separate lines
            return new Matrix(cos, sin,
                              -sin, cos,
                              dx, dy);
            #pragma warning restore SA1117 // Parameters must be on same line or separate lines
        }

        internal static Matrix CreateScaling(double scaleX, double scaleY)
        {
            #pragma warning disable SA1117 // Parameters must be on same line or separate lines
            return new Matrix(scaleX, 0,
                              0, scaleY,
                              0, 0);
            #pragma warning restore SA1117 // Parameters must be on same line or separate lines
        }

        internal static Matrix CreateScaling(double scaleX, double scaleY, double centerX, double centerY)
        {
            #pragma warning disable SA1117 // Parameters must be on same line or separate lines
            return new Matrix(scaleX, 0,
                              0, scaleY,
                              centerX - (scaleX * centerX), centerY - (scaleY * centerY));
            #pragma warning restore SA1117 // Parameters must be on same line or separate lines
        }

        internal static Matrix CreateSkewRadians(double skewX, double skewY)
        {
            #pragma warning disable SA1117 // Parameters must be on same line or separate lines
            return new Matrix(1.0, Math.Tan(skewY),
                              Math.Tan(skewX), 1.0,
                              0.0, 0.0);
            #pragma warning restore SA1117 // Parameters must be on same line or separate lines
        }
    }
}
