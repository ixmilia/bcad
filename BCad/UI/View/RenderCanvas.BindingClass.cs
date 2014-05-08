using System.ComponentModel;
using System.Runtime.CompilerServices;

#if BCAD_METRO
using BCad.Metro.Extensions;
using Windows.UI;
using Windows.UI.Xaml.Media;
#endif

#if BCAD_WPF
using BCad.Extensions;
using System.Dynamic;
using System.Windows.Media;
#endif

namespace BCad.UI.View
{
    public partial class RenderCanvas
    {
        private class BindingClass :
#if BCAD_WPF
            DynamicObject,
#endif
            INotifyPropertyChanged
        {
            private const string BrushText = "Brush";
            private Brush[] brushes;

            public BindingClass()
            {
                brushes = new Brush[ColorMap.ArraySize];
                for (int i = 0; i < ColorMap.ArraySize; i++)
                {
                    brushes[i] = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
            }

#if BCAD_WPF
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (binder.Name.StartsWith(BrushText))
                {
                    int brushNumber;
                    if (int.TryParse(binder.Name.Substring(BrushText.Length), out brushNumber) && brushNumber >= 1 && brushNumber < ColorMap.ArraySize)
                    {
                        result = brushes[brushNumber];
                        return true;
                    }
                }

                result = null;
                return false;
            }
#endif

            private double thickness = 0.0;
            public double Thickness
            {
                get { return thickness; }
                set
                {
                    if (thickness == value)
                        return;
                    thickness = value;
                    OnPropertyChanged();
                }
            }

            private ScaleTransform scale = new ScaleTransform() { ScaleX = 1.0, ScaleY = 1.0 };
            public ScaleTransform Scale
            {
                get { return scale; }
                set
                {
                    if (scale == value)
                        return;
                    scale = value;
                    OnPropertyChanged();
                }
            }

            private Brush autoBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            public Brush AutoBrush
            {
                get { return autoBrush; }
                set
                {
                    if (autoBrush == value)
                        return;
                    autoBrush = value;
                    OnPropertyChanged();
                }
            }

            public void RebindBrushes(ColorMap colorMap)
            {
                for (int i = 1; i < ColorMap.ArraySize; i++)
                {
                    brushes[i] = new SolidColorBrush(colorMap[(byte)i].ToMediaColor());
                    OnPropertyChangedDirect(BrushText + i);
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                OnPropertyChangedDirect(propertyName);
            }

            private void OnPropertyChangedDirect(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }

#if BCAD_METRO
            // WinRT doesn't property use DynamicObject.TryGetMember() so we have to explicitly implement these here.
            public Brush Brush0 { get { return brushes[0]; } }
            public Brush Brush1 { get { return brushes[1]; } }
            public Brush Brush2 { get { return brushes[2]; } }
            public Brush Brush3 { get { return brushes[3]; } }
            public Brush Brush4 { get { return brushes[4]; } }
            public Brush Brush5 { get { return brushes[5]; } }
            public Brush Brush6 { get { return brushes[6]; } }
            public Brush Brush7 { get { return brushes[7]; } }
            public Brush Brush8 { get { return brushes[8]; } }
            public Brush Brush9 { get { return brushes[9]; } }
            public Brush Brush10 { get { return brushes[10]; } }
            public Brush Brush11 { get { return brushes[11]; } }
            public Brush Brush12 { get { return brushes[12]; } }
            public Brush Brush13 { get { return brushes[13]; } }
            public Brush Brush14 { get { return brushes[14]; } }
            public Brush Brush15 { get { return brushes[15]; } }
            public Brush Brush16 { get { return brushes[16]; } }
            public Brush Brush17 { get { return brushes[17]; } }
            public Brush Brush18 { get { return brushes[18]; } }
            public Brush Brush19 { get { return brushes[19]; } }
            public Brush Brush20 { get { return brushes[20]; } }
            public Brush Brush21 { get { return brushes[21]; } }
            public Brush Brush22 { get { return brushes[22]; } }
            public Brush Brush23 { get { return brushes[23]; } }
            public Brush Brush24 { get { return brushes[24]; } }
            public Brush Brush25 { get { return brushes[25]; } }
            public Brush Brush26 { get { return brushes[26]; } }
            public Brush Brush27 { get { return brushes[27]; } }
            public Brush Brush28 { get { return brushes[28]; } }
            public Brush Brush29 { get { return brushes[29]; } }
            public Brush Brush30 { get { return brushes[30]; } }
            public Brush Brush31 { get { return brushes[31]; } }
            public Brush Brush32 { get { return brushes[32]; } }
            public Brush Brush33 { get { return brushes[33]; } }
            public Brush Brush34 { get { return brushes[34]; } }
            public Brush Brush35 { get { return brushes[35]; } }
            public Brush Brush36 { get { return brushes[36]; } }
            public Brush Brush37 { get { return brushes[37]; } }
            public Brush Brush38 { get { return brushes[38]; } }
            public Brush Brush39 { get { return brushes[39]; } }
            public Brush Brush40 { get { return brushes[40]; } }
            public Brush Brush41 { get { return brushes[41]; } }
            public Brush Brush42 { get { return brushes[42]; } }
            public Brush Brush43 { get { return brushes[43]; } }
            public Brush Brush44 { get { return brushes[44]; } }
            public Brush Brush45 { get { return brushes[45]; } }
            public Brush Brush46 { get { return brushes[46]; } }
            public Brush Brush47 { get { return brushes[47]; } }
            public Brush Brush48 { get { return brushes[48]; } }
            public Brush Brush49 { get { return brushes[49]; } }
            public Brush Brush50 { get { return brushes[50]; } }
            public Brush Brush51 { get { return brushes[51]; } }
            public Brush Brush52 { get { return brushes[52]; } }
            public Brush Brush53 { get { return brushes[53]; } }
            public Brush Brush54 { get { return brushes[54]; } }
            public Brush Brush55 { get { return brushes[55]; } }
            public Brush Brush56 { get { return brushes[56]; } }
            public Brush Brush57 { get { return brushes[57]; } }
            public Brush Brush58 { get { return brushes[58]; } }
            public Brush Brush59 { get { return brushes[59]; } }
            public Brush Brush60 { get { return brushes[60]; } }
            public Brush Brush61 { get { return brushes[61]; } }
            public Brush Brush62 { get { return brushes[62]; } }
            public Brush Brush63 { get { return brushes[63]; } }
            public Brush Brush64 { get { return brushes[64]; } }
            public Brush Brush65 { get { return brushes[65]; } }
            public Brush Brush66 { get { return brushes[66]; } }
            public Brush Brush67 { get { return brushes[67]; } }
            public Brush Brush68 { get { return brushes[68]; } }
            public Brush Brush69 { get { return brushes[69]; } }
            public Brush Brush70 { get { return brushes[70]; } }
            public Brush Brush71 { get { return brushes[71]; } }
            public Brush Brush72 { get { return brushes[72]; } }
            public Brush Brush73 { get { return brushes[73]; } }
            public Brush Brush74 { get { return brushes[74]; } }
            public Brush Brush75 { get { return brushes[75]; } }
            public Brush Brush76 { get { return brushes[76]; } }
            public Brush Brush77 { get { return brushes[77]; } }
            public Brush Brush78 { get { return brushes[78]; } }
            public Brush Brush79 { get { return brushes[79]; } }
            public Brush Brush80 { get { return brushes[80]; } }
            public Brush Brush81 { get { return brushes[81]; } }
            public Brush Brush82 { get { return brushes[82]; } }
            public Brush Brush83 { get { return brushes[83]; } }
            public Brush Brush84 { get { return brushes[84]; } }
            public Brush Brush85 { get { return brushes[85]; } }
            public Brush Brush86 { get { return brushes[86]; } }
            public Brush Brush87 { get { return brushes[87]; } }
            public Brush Brush88 { get { return brushes[88]; } }
            public Brush Brush89 { get { return brushes[89]; } }
            public Brush Brush90 { get { return brushes[90]; } }
            public Brush Brush91 { get { return brushes[91]; } }
            public Brush Brush92 { get { return brushes[92]; } }
            public Brush Brush93 { get { return brushes[93]; } }
            public Brush Brush94 { get { return brushes[94]; } }
            public Brush Brush95 { get { return brushes[95]; } }
            public Brush Brush96 { get { return brushes[96]; } }
            public Brush Brush97 { get { return brushes[97]; } }
            public Brush Brush98 { get { return brushes[98]; } }
            public Brush Brush99 { get { return brushes[99]; } }
            public Brush Brush100 { get { return brushes[100]; } }
            public Brush Brush101 { get { return brushes[101]; } }
            public Brush Brush102 { get { return brushes[102]; } }
            public Brush Brush103 { get { return brushes[103]; } }
            public Brush Brush104 { get { return brushes[104]; } }
            public Brush Brush105 { get { return brushes[105]; } }
            public Brush Brush106 { get { return brushes[106]; } }
            public Brush Brush107 { get { return brushes[107]; } }
            public Brush Brush108 { get { return brushes[108]; } }
            public Brush Brush109 { get { return brushes[109]; } }
            public Brush Brush110 { get { return brushes[110]; } }
            public Brush Brush111 { get { return brushes[111]; } }
            public Brush Brush112 { get { return brushes[112]; } }
            public Brush Brush113 { get { return brushes[113]; } }
            public Brush Brush114 { get { return brushes[114]; } }
            public Brush Brush115 { get { return brushes[115]; } }
            public Brush Brush116 { get { return brushes[116]; } }
            public Brush Brush117 { get { return brushes[117]; } }
            public Brush Brush118 { get { return brushes[118]; } }
            public Brush Brush119 { get { return brushes[119]; } }
            public Brush Brush120 { get { return brushes[120]; } }
            public Brush Brush121 { get { return brushes[121]; } }
            public Brush Brush122 { get { return brushes[122]; } }
            public Brush Brush123 { get { return brushes[123]; } }
            public Brush Brush124 { get { return brushes[124]; } }
            public Brush Brush125 { get { return brushes[125]; } }
            public Brush Brush126 { get { return brushes[126]; } }
            public Brush Brush127 { get { return brushes[127]; } }
            public Brush Brush128 { get { return brushes[128]; } }
            public Brush Brush129 { get { return brushes[129]; } }
            public Brush Brush130 { get { return brushes[130]; } }
            public Brush Brush131 { get { return brushes[131]; } }
            public Brush Brush132 { get { return brushes[132]; } }
            public Brush Brush133 { get { return brushes[133]; } }
            public Brush Brush134 { get { return brushes[134]; } }
            public Brush Brush135 { get { return brushes[135]; } }
            public Brush Brush136 { get { return brushes[136]; } }
            public Brush Brush137 { get { return brushes[137]; } }
            public Brush Brush138 { get { return brushes[138]; } }
            public Brush Brush139 { get { return brushes[139]; } }
            public Brush Brush140 { get { return brushes[140]; } }
            public Brush Brush141 { get { return brushes[141]; } }
            public Brush Brush142 { get { return brushes[142]; } }
            public Brush Brush143 { get { return brushes[143]; } }
            public Brush Brush144 { get { return brushes[144]; } }
            public Brush Brush145 { get { return brushes[145]; } }
            public Brush Brush146 { get { return brushes[146]; } }
            public Brush Brush147 { get { return brushes[147]; } }
            public Brush Brush148 { get { return brushes[148]; } }
            public Brush Brush149 { get { return brushes[149]; } }
            public Brush Brush150 { get { return brushes[150]; } }
            public Brush Brush151 { get { return brushes[151]; } }
            public Brush Brush152 { get { return brushes[152]; } }
            public Brush Brush153 { get { return brushes[153]; } }
            public Brush Brush154 { get { return brushes[154]; } }
            public Brush Brush155 { get { return brushes[155]; } }
            public Brush Brush156 { get { return brushes[156]; } }
            public Brush Brush157 { get { return brushes[157]; } }
            public Brush Brush158 { get { return brushes[158]; } }
            public Brush Brush159 { get { return brushes[159]; } }
            public Brush Brush160 { get { return brushes[160]; } }
            public Brush Brush161 { get { return brushes[161]; } }
            public Brush Brush162 { get { return brushes[162]; } }
            public Brush Brush163 { get { return brushes[163]; } }
            public Brush Brush164 { get { return brushes[164]; } }
            public Brush Brush165 { get { return brushes[165]; } }
            public Brush Brush166 { get { return brushes[166]; } }
            public Brush Brush167 { get { return brushes[167]; } }
            public Brush Brush168 { get { return brushes[168]; } }
            public Brush Brush169 { get { return brushes[169]; } }
            public Brush Brush170 { get { return brushes[170]; } }
            public Brush Brush171 { get { return brushes[171]; } }
            public Brush Brush172 { get { return brushes[172]; } }
            public Brush Brush173 { get { return brushes[173]; } }
            public Brush Brush174 { get { return brushes[174]; } }
            public Brush Brush175 { get { return brushes[175]; } }
            public Brush Brush176 { get { return brushes[176]; } }
            public Brush Brush177 { get { return brushes[177]; } }
            public Brush Brush178 { get { return brushes[178]; } }
            public Brush Brush179 { get { return brushes[179]; } }
            public Brush Brush180 { get { return brushes[180]; } }
            public Brush Brush181 { get { return brushes[181]; } }
            public Brush Brush182 { get { return brushes[182]; } }
            public Brush Brush183 { get { return brushes[183]; } }
            public Brush Brush184 { get { return brushes[184]; } }
            public Brush Brush185 { get { return brushes[185]; } }
            public Brush Brush186 { get { return brushes[186]; } }
            public Brush Brush187 { get { return brushes[187]; } }
            public Brush Brush188 { get { return brushes[188]; } }
            public Brush Brush189 { get { return brushes[189]; } }
            public Brush Brush190 { get { return brushes[190]; } }
            public Brush Brush191 { get { return brushes[191]; } }
            public Brush Brush192 { get { return brushes[192]; } }
            public Brush Brush193 { get { return brushes[193]; } }
            public Brush Brush194 { get { return brushes[194]; } }
            public Brush Brush195 { get { return brushes[195]; } }
            public Brush Brush196 { get { return brushes[196]; } }
            public Brush Brush197 { get { return brushes[197]; } }
            public Brush Brush198 { get { return brushes[198]; } }
            public Brush Brush199 { get { return brushes[199]; } }
            public Brush Brush200 { get { return brushes[200]; } }
            public Brush Brush201 { get { return brushes[201]; } }
            public Brush Brush202 { get { return brushes[202]; } }
            public Brush Brush203 { get { return brushes[203]; } }
            public Brush Brush204 { get { return brushes[204]; } }
            public Brush Brush205 { get { return brushes[205]; } }
            public Brush Brush206 { get { return brushes[206]; } }
            public Brush Brush207 { get { return brushes[207]; } }
            public Brush Brush208 { get { return brushes[208]; } }
            public Brush Brush209 { get { return brushes[209]; } }
            public Brush Brush210 { get { return brushes[210]; } }
            public Brush Brush211 { get { return brushes[211]; } }
            public Brush Brush212 { get { return brushes[212]; } }
            public Brush Brush213 { get { return brushes[213]; } }
            public Brush Brush214 { get { return brushes[214]; } }
            public Brush Brush215 { get { return brushes[215]; } }
            public Brush Brush216 { get { return brushes[216]; } }
            public Brush Brush217 { get { return brushes[217]; } }
            public Brush Brush218 { get { return brushes[218]; } }
            public Brush Brush219 { get { return brushes[219]; } }
            public Brush Brush220 { get { return brushes[220]; } }
            public Brush Brush221 { get { return brushes[221]; } }
            public Brush Brush222 { get { return brushes[222]; } }
            public Brush Brush223 { get { return brushes[223]; } }
            public Brush Brush224 { get { return brushes[224]; } }
            public Brush Brush225 { get { return brushes[225]; } }
            public Brush Brush226 { get { return brushes[226]; } }
            public Brush Brush227 { get { return brushes[227]; } }
            public Brush Brush228 { get { return brushes[228]; } }
            public Brush Brush229 { get { return brushes[229]; } }
            public Brush Brush230 { get { return brushes[230]; } }
            public Brush Brush231 { get { return brushes[231]; } }
            public Brush Brush232 { get { return brushes[232]; } }
            public Brush Brush233 { get { return brushes[233]; } }
            public Brush Brush234 { get { return brushes[234]; } }
            public Brush Brush235 { get { return brushes[235]; } }
            public Brush Brush236 { get { return brushes[236]; } }
            public Brush Brush237 { get { return brushes[237]; } }
            public Brush Brush238 { get { return brushes[238]; } }
            public Brush Brush239 { get { return brushes[239]; } }
            public Brush Brush240 { get { return brushes[240]; } }
            public Brush Brush241 { get { return brushes[241]; } }
            public Brush Brush242 { get { return brushes[242]; } }
            public Brush Brush243 { get { return brushes[243]; } }
            public Brush Brush244 { get { return brushes[244]; } }
            public Brush Brush245 { get { return brushes[245]; } }
            public Brush Brush246 { get { return brushes[246]; } }
            public Brush Brush247 { get { return brushes[247]; } }
            public Brush Brush248 { get { return brushes[248]; } }
            public Brush Brush249 { get { return brushes[249]; } }
            public Brush Brush250 { get { return brushes[250]; } }
            public Brush Brush251 { get { return brushes[251]; } }
            public Brush Brush252 { get { return brushes[252]; } }
            public Brush Brush253 { get { return brushes[253]; } }
            public Brush Brush254 { get { return brushes[254]; } }
            public Brush Brush255 { get { return brushes[255]; } }
#endif
        }
    }
}
