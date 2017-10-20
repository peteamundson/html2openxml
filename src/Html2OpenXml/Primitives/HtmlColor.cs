﻿/* Copyright (C) Olivier Nizet https://github.com/onizet/html2openxml - All Rights Reserved
 * 
 * This source is subject to the Microsoft Permissive License.
 * Please see the License.txt file for more information.
 * All other rights reserved.
 * 
 * THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
 * KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
 * PARTICULAR PURPOSE.
 */
using System;
using System.Globalization;

namespace HtmlToOpenXml
{
    /// <summary>
    /// Represents an ARGB color.
    /// </summary>
    public struct HtmlColor
    {
        private static readonly char[] hexDigits = {
         '0', '1', '2', '3', '4', '5', '6', '7',
         '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

        /// <summary>
        /// Represents a color that is null.
        /// </summary>
        public static readonly HtmlColor Empty = new HtmlColor();
        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF000000.
        /// </summary>
        public static readonly HtmlColor Black = FromArgb(0, 0, 0);


        /// <summary>
        /// Try to parse a value (RGB(A) or HSL(A), hexadecimal, or named color) to its RGB representation.
        /// </summary>
        /// <param name="htmlColor">The color to parse.</param>
        /// <returns>Returns <see cref="HtmlColor.Empty"/> if parsing failed.</returns>
        public static HtmlColor Parse(string htmlColor)
        {
            if (string.IsNullOrEmpty(htmlColor))
                return HtmlColor.Empty;

            // Bug fixed by jairoXXX to support rgb(r,g,b) format
            // RGB or RGBA
            if (htmlColor.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
            {
                int startIndex = htmlColor.IndexOf('(', 3), endIndex = htmlColor.LastIndexOf(')');
                if (startIndex >= 3 && endIndex > -1)
                {
                    var colorStringArray = htmlColor.Substring(startIndex + 1, endIndex - startIndex - 1).Split(',');
                    if (colorStringArray.Length >= 3)
                    {
                        return FromArgb(
                            colorStringArray.Length == 3 ? 1.0: double.Parse(colorStringArray[0], CultureInfo.InvariantCulture),
                            Byte.Parse(colorStringArray[colorStringArray.Length - 3], NumberStyles.Integer, CultureInfo.InvariantCulture),
                            Byte.Parse(colorStringArray[colorStringArray.Length - 2], NumberStyles.Integer, CultureInfo.InvariantCulture),
                            Byte.Parse(colorStringArray[colorStringArray.Length - 1], NumberStyles.Integer, CultureInfo.InvariantCulture)
                        );
                    }
                }
            }

            // HSL or HSLA
            if (htmlColor.StartsWith("hsl", StringComparison.OrdinalIgnoreCase))
            {
                int startIndex = htmlColor.IndexOf('(', 3), endIndex = htmlColor.LastIndexOf(')');
                if (startIndex >= 3 && endIndex > -1)
                {
                    var colorStringArray = htmlColor.Substring(startIndex + 1, endIndex - startIndex - 1).Split(',');
                    if (colorStringArray.Length >= 3)
                    {
                        return FromHsl(
                            colorStringArray.Length == 3 ? 255d: double.Parse(colorStringArray[0], CultureInfo.InvariantCulture),
                            double.Parse(colorStringArray[colorStringArray.Length - 3], CultureInfo.InvariantCulture),
                            double.Parse(colorStringArray[colorStringArray.Length - 2], CultureInfo.InvariantCulture),
                            double.Parse(colorStringArray[colorStringArray.Length - 1], CultureInfo.InvariantCulture)
                        );
                    }
                }
            }

            // Is it in hexa? Note: we no more accept hexa value without preceding the '#'
            if (htmlColor[0] == '#' && (htmlColor.Length == 7 || htmlColor.Length == 4))
            {
                try
                {
                    if (htmlColor.Length == 7)
                    {
                        return FromArgb(
                            Convert.ToByte(htmlColor.Substring(1, 2), 16),
                            Convert.ToByte(htmlColor.Substring(3, 2), 16),
                            Convert.ToByte(htmlColor.Substring(5, 2), 16));
                    }

                    // #0FF --> #00FFFF
                    return FromArgb(
                            Convert.ToByte(new string(htmlColor[1], 2), 16),
                            Convert.ToByte(new string(htmlColor[2], 2), 16),
                            Convert.ToByte(new string(htmlColor[3], 2), 16));
                }
                catch (System.FormatException)
                {
                    // If the conversion failed, that should be a named color
                    // Let's the framework dealing with it
                }
            }

            return HtmlColorTranslator.FromHtml(htmlColor);
        }

        /// <summary>
        /// Creates a <see cref="HtmlColor"/> structure from the four RGB component values.
        /// </summary>
        /// <param name="red">The red component.</param>
        /// <param name="green">The green component.</param>
        /// <param name="blue">The blue component.</param>
        public static HtmlColor FromArgb(byte red, byte green, byte blue)
        {
            return FromArgb(0d, red, green, blue);
        }

        /// <summary>
        /// Creates a <see cref="HtmlColor"/> structure from the four ARGB component values.
        /// </summary>
        /// <param name="alpha">The alpha component (0.0-1.0).</param>
        /// <param name="red">The red component (0-255).</param>
        /// <param name="green">The green component (0-255).</param>
        /// <param name="blue">The blue component (0-255).</param>
        public static HtmlColor FromArgb(double alpha, byte red, byte green, byte blue)
        {
            if (alpha < 0.0 || alpha > 1.0)
                throw new ArgumentOutOfRangeException(nameof(alpha), alpha, "Alpha should be comprised between 0.0 and 1.0");

            return new HtmlColor() {
                A = alpha, R = red, G = green, B = blue
            };
        }

        /// <summary>
        /// Convert a color using the HSL to RGB.
        /// </summary>
        /// <param name="alpha">The alpha component (0.0-1.0).</param>
        /// <param name="hue">The Hue component (0.0 - 1.0).</param>
        /// <param name="saturation">The saturation component (0.0 - 1.0).</param>
        /// <param name="luminosity">The luminosity component (0.0 - 1.0).</param>
        public static HtmlColor FromHsl(double alpha, double hue, double saturation, double luminosity)
        {
            if (0 > alpha || 255 < alpha)
            {
                throw new ArgumentOutOfRangeException(nameof(alpha), alpha, "Alpha should be comprised between 0.0 and 1.0");
            }
            if (0f > hue || 360f < hue)
            {
                throw new ArgumentOutOfRangeException(nameof(hue), hue, "Hue should be comprised between 0° and 360°");
            }
            if (0f > saturation || 1f < saturation)
            {
                throw new ArgumentOutOfRangeException(nameof(saturation), saturation, "Saturation should be comprised between 0.0 and 1.0");
            }
            if (0f > luminosity || 1f < luminosity)
            {
                throw new ArgumentOutOfRangeException(nameof(luminosity), luminosity, "Brightness should be comprised between 0.0 and 1.0");
            }

            if (0 == saturation)
            {
                return HtmlColor.FromArgb(alpha, Convert.ToByte(luminosity * 255),
                  Convert.ToByte(luminosity * 255), Convert.ToByte(luminosity * 255));
            }

            double fMax, fMid, fMin;
            int iSextant;

            if (0.5 < luminosity)
            {
                fMax = luminosity - (luminosity * saturation) + saturation;
                fMin = luminosity + (luminosity * saturation) - saturation;
            }
            else
            {
                fMax = luminosity + (luminosity * saturation);
                fMin = luminosity - (luminosity * saturation);
            }

            iSextant = (int) Math.Floor(hue / 60f);
            if (300f <= hue)
            {
                hue -= 360f;
            }
            hue /= 60f;
            hue -= 2f * (float) Math.Floor(((iSextant + 1f) % 6f) / 2f);
            if (0 == iSextant % 2)
            {
                fMid = hue * (fMax - fMin) + fMin;
            }
            else
            {
                fMid = fMin - hue * (fMax - fMin);
            }

            byte iMax = Convert.ToByte(fMax * 255);
            byte iMid = Convert.ToByte(fMid * 255);
            byte iMin = Convert.ToByte(fMin * 255);

            switch (iSextant)
            {
                case 1:
                    return HtmlColor.FromArgb(alpha, iMid, iMax, iMin);
                case 2:
                    return HtmlColor.FromArgb(alpha, iMin, iMax, iMid);
                case 3:
                    return HtmlColor.FromArgb(alpha, iMin, iMid, iMax);
                case 4:
                    return HtmlColor.FromArgb(alpha, iMid, iMin, iMax);
                case 5:
                    return HtmlColor.FromArgb(alpha, iMax, iMin, iMid);
                default:
                    return HtmlColor.FromArgb(alpha, iMax, iMid, iMin);
            }
        }

        /// <summary>
        /// Tests whether the specified object is a HtmlColor structure and is equivalent to this color structure.
        /// </summary>
        public bool Equals(HtmlColor color)
        {
            return color.A == A && color.R == A && color.G == A && color.B == A;
        }

        /// <summary>
        /// Convert a .Net Color to a hex string.
        /// </summary>
        public string ToHexString()
        {
            // http://www.cambiaresearch.com/c4/24c09e15-2941-4ad2-8695-00b1b4029f4d/Convert-dotnet-Color-to-Hex-String.aspx

            byte[] bytes = new byte[3];
            bytes[0] = this.R;
            bytes[1] = this.G;
            bytes[2] = this.B;
            char[] chars = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i * 2] = hexDigits[b >> 4];
                chars[i * 2 + 1] = hexDigits[b & 0xF];
            }
            return new string(chars);
        }

        /// <summary>
        /// Gets a representation of this color expressed in ARGB.
        /// </summary>
        public override string ToString()
        {
            return String.Format("A: {0:#0.##} R: {1:#0##} G: {2:#0##} B: {3:#0##}", this.A, this.R, this.G, this.B);
        }

        //____________________________________________________________________
        //

        /// <summary>Gets the alpha component value of this color structure.</summary>
        public double A { get; set; }
        /// <summary>Gets the red component value of this cColor structure.</summary>
        public byte R { get; set; }
        /// <summary>Gets the green component value of this color structure.</summary>
        public byte G { get; set; }
        /// <summary>Gets the blue component value of this color structure.</summary>
        public byte B { get; set; }

        /// <summary>
        /// Specifies whether this HtmlColor structure is uninitialized.
        /// </summary>
        public bool IsEmpty { get { return this.Equals(Empty); } }
    }
}