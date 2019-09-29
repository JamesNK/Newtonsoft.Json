#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#if !(NETSTANDARD1_0 || NETSTANDARD1_3)
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue2176
    {
        [Test]
        public void Test()
        {
            Dummy o = JsonConvert.DeserializeObject<Dummy>(Json);
            Assert.AreEqual(0, o.P0);
            Assert.AreEqual(256, o.P256);
        }

        public class Dummy
        {
            public int P0 { get; }
            public int P1 { get; }
            public int P2 { get; }
            public int P3 { get; }
            public int P4 { get; }
            public int P5 { get; }
            public int P6 { get; }
            public int P7 { get; }
            public int P8 { get; }
            public int P9 { get; }
            public int P10 { get; }
            public int P11 { get; }
            public int P12 { get; }
            public int P13 { get; }
            public int P14 { get; }
            public int P15 { get; }
            public int P16 { get; }
            public int P17 { get; }
            public int P18 { get; }
            public int P19 { get; }
            public int P20 { get; }
            public int P21 { get; }
            public int P22 { get; }
            public int P23 { get; }
            public int P24 { get; }
            public int P25 { get; }
            public int P26 { get; }
            public int P27 { get; }
            public int P28 { get; }
            public int P29 { get; }
            public int P30 { get; }
            public int P31 { get; }
            public int P32 { get; }
            public int P33 { get; }
            public int P34 { get; }
            public int P35 { get; }
            public int P36 { get; }
            public int P37 { get; }
            public int P38 { get; }
            public int P39 { get; }
            public int P40 { get; }
            public int P41 { get; }
            public int P42 { get; }
            public int P43 { get; }
            public int P44 { get; }
            public int P45 { get; }
            public int P46 { get; }
            public int P47 { get; }
            public int P48 { get; }
            public int P49 { get; }
            public int P50 { get; }
            public int P51 { get; }
            public int P52 { get; }
            public int P53 { get; }
            public int P54 { get; }
            public int P55 { get; }
            public int P56 { get; }
            public int P57 { get; }
            public int P58 { get; }
            public int P59 { get; }
            public int P60 { get; }
            public int P61 { get; }
            public int P62 { get; }
            public int P63 { get; }
            public int P64 { get; }
            public int P65 { get; }
            public int P66 { get; }
            public int P67 { get; }
            public int P68 { get; }
            public int P69 { get; }
            public int P70 { get; }
            public int P71 { get; }
            public int P72 { get; }
            public int P73 { get; }
            public int P74 { get; }
            public int P75 { get; }
            public int P76 { get; }
            public int P77 { get; }
            public int P78 { get; }
            public int P79 { get; }
            public int P80 { get; }
            public int P81 { get; }
            public int P82 { get; }
            public int P83 { get; }
            public int P84 { get; }
            public int P85 { get; }
            public int P86 { get; }
            public int P87 { get; }
            public int P88 { get; }
            public int P89 { get; }
            public int P90 { get; }
            public int P91 { get; }
            public int P92 { get; }
            public int P93 { get; }
            public int P94 { get; }
            public int P95 { get; }
            public int P96 { get; }
            public int P97 { get; }
            public int P98 { get; }
            public int P99 { get; }
            public int P100 { get; }
            public int P101 { get; }
            public int P102 { get; }
            public int P103 { get; }
            public int P104 { get; }
            public int P105 { get; }
            public int P106 { get; }
            public int P107 { get; }
            public int P108 { get; }
            public int P109 { get; }
            public int P110 { get; }
            public int P111 { get; }
            public int P112 { get; }
            public int P113 { get; }
            public int P114 { get; }
            public int P115 { get; }
            public int P116 { get; }
            public int P117 { get; }
            public int P118 { get; }
            public int P119 { get; }
            public int P120 { get; }
            public int P121 { get; }
            public int P122 { get; }
            public int P123 { get; }
            public int P124 { get; }
            public int P125 { get; }
            public int P126 { get; }
            public int P127 { get; }
            public int P128 { get; }
            public int P129 { get; }
            public int P130 { get; }
            public int P131 { get; }
            public int P132 { get; }
            public int P133 { get; }
            public int P134 { get; }
            public int P135 { get; }
            public int P136 { get; }
            public int P137 { get; }
            public int P138 { get; }
            public int P139 { get; }
            public int P140 { get; }
            public int P141 { get; }
            public int P142 { get; }
            public int P143 { get; }
            public int P144 { get; }
            public int P145 { get; }
            public int P146 { get; }
            public int P147 { get; }
            public int P148 { get; }
            public int P149 { get; }
            public int P150 { get; }
            public int P151 { get; }
            public int P152 { get; }
            public int P153 { get; }
            public int P154 { get; }
            public int P155 { get; }
            public int P156 { get; }
            public int P157 { get; }
            public int P158 { get; }
            public int P159 { get; }
            public int P160 { get; }
            public int P161 { get; }
            public int P162 { get; }
            public int P163 { get; }
            public int P164 { get; }
            public int P165 { get; }
            public int P166 { get; }
            public int P167 { get; }
            public int P168 { get; }
            public int P169 { get; }
            public int P170 { get; }
            public int P171 { get; }
            public int P172 { get; }
            public int P173 { get; }
            public int P174 { get; }
            public int P175 { get; }
            public int P176 { get; }
            public int P177 { get; }
            public int P178 { get; }
            public int P179 { get; }
            public int P180 { get; }
            public int P181 { get; }
            public int P182 { get; }
            public int P183 { get; }
            public int P184 { get; }
            public int P185 { get; }
            public int P186 { get; }
            public int P187 { get; }
            public int P188 { get; }
            public int P189 { get; }
            public int P190 { get; }
            public int P191 { get; }
            public int P192 { get; }
            public int P193 { get; }
            public int P194 { get; }
            public int P195 { get; }
            public int P196 { get; }
            public int P197 { get; }
            public int P198 { get; }
            public int P199 { get; }
            public int P200 { get; }
            public int P201 { get; }
            public int P202 { get; }
            public int P203 { get; }
            public int P204 { get; }
            public int P205 { get; }
            public int P206 { get; }
            public int P207 { get; }
            public int P208 { get; }
            public int P209 { get; }
            public int P210 { get; }
            public int P211 { get; }
            public int P212 { get; }
            public int P213 { get; }
            public int P214 { get; }
            public int P215 { get; }
            public int P216 { get; }
            public int P217 { get; }
            public int P218 { get; }
            public int P219 { get; }
            public int P220 { get; }
            public int P221 { get; }
            public int P222 { get; }
            public int P223 { get; }
            public int P224 { get; }
            public int P225 { get; }
            public int P226 { get; }
            public int P227 { get; }
            public int P228 { get; }
            public int P229 { get; }
            public int P230 { get; }
            public int P231 { get; }
            public int P232 { get; }
            public int P233 { get; }
            public int P234 { get; }
            public int P235 { get; }
            public int P236 { get; }
            public int P237 { get; }
            public int P238 { get; }
            public int P239 { get; }
            public int P240 { get; }
            public int P241 { get; }
            public int P242 { get; }
            public int P243 { get; }
            public int P244 { get; }
            public int P245 { get; }
            public int P246 { get; }
            public int P247 { get; }
            public int P248 { get; }
            public int P249 { get; }
            public int P250 { get; }
            public int P251 { get; }
            public int P252 { get; }
            public int P253 { get; }
            public int P254 { get; }
            public int P255 { get; }
            public int P256 { get; }

            public Dummy(int p0, int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9, int p10, int p11, int p12, int p13, int p14, int p15, int p16, int p17, int p18, int p19, int p20, int p21, int p22, int p23, int p24, int p25, int p26, int p27, int p28, int p29, int p30, int p31, int p32, int p33, int p34, int p35, int p36, int p37, int p38, int p39, int p40, int p41, int p42, int p43, int p44, int p45, int p46, int p47, int p48, int p49, int p50, int p51, int p52, int p53, int p54, int p55, int p56, int p57, int p58, int p59, int p60, int p61, int p62, int p63, int p64, int p65, int p66, int p67, int p68, int p69, int p70, int p71, int p72, int p73, int p74, int p75, int p76, int p77, int p78, int p79, int p80, int p81, int p82, int p83, int p84, int p85, int p86, int p87, int p88, int p89, int p90, int p91, int p92, int p93, int p94, int p95, int p96, int p97, int p98, int p99, int p100, int p101, int p102, int p103, int p104, int p105, int p106, int p107, int p108, int p109, int p110, int p111, int p112, int p113, int p114, int p115, int p116, int p117, int p118, int p119, int p120, int p121, int p122, int p123, int p124, int p125, int p126, int p127, int p128, int p129, int p130, int p131, int p132, int p133, int p134, int p135, int p136, int p137, int p138, int p139, int p140, int p141, int p142, int p143, int p144, int p145, int p146, int p147, int p148, int p149, int p150, int p151, int p152, int p153, int p154, int p155, int p156, int p157, int p158, int p159, int p160, int p161, int p162, int p163, int p164, int p165, int p166, int p167, int p168, int p169, int p170, int p171, int p172, int p173, int p174, int p175, int p176, int p177, int p178, int p179, int p180, int p181, int p182, int p183, int p184, int p185, int p186, int p187, int p188, int p189, int p190, int p191, int p192, int p193, int p194, int p195, int p196, int p197, int p198, int p199, int p200, int p201, int p202, int p203, int p204, int p205, int p206, int p207, int p208, int p209, int p210, int p211, int p212, int p213, int p214, int p215, int p216, int p217, int p218, int p219, int p220, int p221, int p222, int p223, int p224, int p225, int p226, int p227, int p228, int p229, int p230, int p231, int p232, int p233, int p234, int p235, int p236, int p237, int p238, int p239, int p240, int p241, int p242, int p243, int p244, int p245, int p246, int p247, int p248, int p249, int p250, int p251, int p252, int p253, int p254, int p255, int p256)
            {
                P0 = p0;
                P256 = p256;
            }
        }

        public const string Json = @"{
    ""p0"" : 0,
    ""p1"" : 1,
    ""p2"" : 2,
    ""p3"" : 3,
    ""p4"" : 4,
    ""p5"" : 5,
    ""p6"" : 6,
    ""p7"" : 7,
    ""p8"" : 8,
    ""p9"" : 9,
    ""p10"" : 10,
    ""p11"" : 11,
    ""p12"" : 12,
    ""p13"" : 13,
    ""p14"" : 14,
    ""p15"" : 15,
    ""p16"" : 16,
    ""p17"" : 17,
    ""p18"" : 18,
    ""p19"" : 19,
    ""p20"" : 20,
    ""p21"" : 21,
    ""p22"" : 22,
    ""p23"" : 23,
    ""p24"" : 24,
    ""p25"" : 25,
    ""p26"" : 26,
    ""p27"" : 27,
    ""p28"" : 28,
    ""p29"" : 29,
    ""p30"" : 30,
    ""p31"" : 31,
    ""p32"" : 32,
    ""p33"" : 33,
    ""p34"" : 34,
    ""p35"" : 35,
    ""p36"" : 36,
    ""p37"" : 37,
    ""p38"" : 38,
    ""p39"" : 39,
    ""p40"" : 40,
    ""p41"" : 41,
    ""p42"" : 42,
    ""p43"" : 43,
    ""p44"" : 44,
    ""p45"" : 45,
    ""p46"" : 46,
    ""p47"" : 47,
    ""p48"" : 48,
    ""p49"" : 49,
    ""p50"" : 50,
    ""p51"" : 51,
    ""p52"" : 52,
    ""p53"" : 53,
    ""p54"" : 54,
    ""p55"" : 55,
    ""p56"" : 56,
    ""p57"" : 57,
    ""p58"" : 58,
    ""p59"" : 59,
    ""p60"" : 60,
    ""p61"" : 61,
    ""p62"" : 62,
    ""p63"" : 63,
    ""p64"" : 64,
    ""p65"" : 65,
    ""p66"" : 66,
    ""p67"" : 67,
    ""p68"" : 68,
    ""p69"" : 69,
    ""p70"" : 70,
    ""p71"" : 71,
    ""p72"" : 72,
    ""p73"" : 73,
    ""p74"" : 74,
    ""p75"" : 75,
    ""p76"" : 76,
    ""p77"" : 77,
    ""p78"" : 78,
    ""p79"" : 79,
    ""p80"" : 80,
    ""p81"" : 81,
    ""p82"" : 82,
    ""p83"" : 83,
    ""p84"" : 84,
    ""p85"" : 85,
    ""p86"" : 86,
    ""p87"" : 87,
    ""p88"" : 88,
    ""p89"" : 89,
    ""p90"" : 90,
    ""p91"" : 91,
    ""p92"" : 92,
    ""p93"" : 93,
    ""p94"" : 94,
    ""p95"" : 95,
    ""p96"" : 96,
    ""p97"" : 97,
    ""p98"" : 98,
    ""p99"" : 99,
    ""p100"" : 100,
    ""p101"" : 101,
    ""p102"" : 102,
    ""p103"" : 103,
    ""p104"" : 104,
    ""p105"" : 105,
    ""p106"" : 106,
    ""p107"" : 107,
    ""p108"" : 108,
    ""p109"" : 109,
    ""p110"" : 110,
    ""p111"" : 111,
    ""p112"" : 112,
    ""p113"" : 113,
    ""p114"" : 114,
    ""p115"" : 115,
    ""p116"" : 116,
    ""p117"" : 117,
    ""p118"" : 118,
    ""p119"" : 119,
    ""p120"" : 120,
    ""p121"" : 121,
    ""p122"" : 122,
    ""p123"" : 123,
    ""p124"" : 124,
    ""p125"" : 125,
    ""p126"" : 126,
    ""p127"" : 127,
    ""p128"" : 128,
    ""p129"" : 129,
    ""p130"" : 130,
    ""p131"" : 131,
    ""p132"" : 132,
    ""p133"" : 133,
    ""p134"" : 134,
    ""p135"" : 135,
    ""p136"" : 136,
    ""p137"" : 137,
    ""p138"" : 138,
    ""p139"" : 139,
    ""p140"" : 140,
    ""p141"" : 141,
    ""p142"" : 142,
    ""p143"" : 143,
    ""p144"" : 144,
    ""p145"" : 145,
    ""p146"" : 146,
    ""p147"" : 147,
    ""p148"" : 148,
    ""p149"" : 149,
    ""p150"" : 150,
    ""p151"" : 151,
    ""p152"" : 152,
    ""p153"" : 153,
    ""p154"" : 154,
    ""p155"" : 155,
    ""p156"" : 156,
    ""p157"" : 157,
    ""p158"" : 158,
    ""p159"" : 159,
    ""p160"" : 160,
    ""p161"" : 161,
    ""p162"" : 162,
    ""p163"" : 163,
    ""p164"" : 164,
    ""p165"" : 165,
    ""p166"" : 166,
    ""p167"" : 167,
    ""p168"" : 168,
    ""p169"" : 169,
    ""p170"" : 170,
    ""p171"" : 171,
    ""p172"" : 172,
    ""p173"" : 173,
    ""p174"" : 174,
    ""p175"" : 175,
    ""p176"" : 176,
    ""p177"" : 177,
    ""p178"" : 178,
    ""p179"" : 179,
    ""p180"" : 180,
    ""p181"" : 181,
    ""p182"" : 182,
    ""p183"" : 183,
    ""p184"" : 184,
    ""p185"" : 185,
    ""p186"" : 186,
    ""p187"" : 187,
    ""p188"" : 188,
    ""p189"" : 189,
    ""p190"" : 190,
    ""p191"" : 191,
    ""p192"" : 192,
    ""p193"" : 193,
    ""p194"" : 194,
    ""p195"" : 195,
    ""p196"" : 196,
    ""p197"" : 197,
    ""p198"" : 198,
    ""p199"" : 199,
    ""p200"" : 200,
    ""p201"" : 201,
    ""p202"" : 202,
    ""p203"" : 203,
    ""p204"" : 204,
    ""p205"" : 205,
    ""p206"" : 206,
    ""p207"" : 207,
    ""p208"" : 208,
    ""p209"" : 209,
    ""p210"" : 210,
    ""p211"" : 211,
    ""p212"" : 212,
    ""p213"" : 213,
    ""p214"" : 214,
    ""p215"" : 215,
    ""p216"" : 216,
    ""p217"" : 217,
    ""p218"" : 218,
    ""p219"" : 219,
    ""p220"" : 220,
    ""p221"" : 221,
    ""p222"" : 222,
    ""p223"" : 223,
    ""p224"" : 224,
    ""p225"" : 225,
    ""p226"" : 226,
    ""p227"" : 227,
    ""p228"" : 228,
    ""p229"" : 229,
    ""p230"" : 230,
    ""p231"" : 231,
    ""p232"" : 232,
    ""p233"" : 233,
    ""p234"" : 234,
    ""p235"" : 235,
    ""p236"" : 236,
    ""p237"" : 237,
    ""p238"" : 238,
    ""p239"" : 239,
    ""p240"" : 240,
    ""p241"" : 241,
    ""p242"" : 242,
    ""p243"" : 243,
    ""p244"" : 244,
    ""p245"" : 245,
    ""p246"" : 246,
    ""p247"" : 247,
    ""p248"" : 248,
    ""p249"" : 249,
    ""p250"" : 250,
    ""p251"" : 251,
    ""p252"" : 252,
    ""p253"" : 253,
    ""p254"" : 254,
    ""p255"" : 255,
    ""p256"" : 256,
}";
    }
}
#endif