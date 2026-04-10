using UnityEngine;

namespace Deepwave.ICR.Data
{
    internal readonly struct BuildSettings
    {
        public readonly float PanelWidth;
        public readonly float PanelLength;
        public readonly float VerticalOverlap;
        public readonly float OverlapDistance;
        public readonly int Columns;
        public readonly int Rows;

        public readonly AnimationCurve WaveProfile;
        public readonly int WaveCountPerPanel;
        public readonly float WaveAmplitude;
        public readonly float Thickness;

        public readonly TransformSettings PanelTransform;
        public readonly NoiseSettings Noise;
        public readonly SplineSettings Spline;
        public readonly CutSettings Cuts;
        public readonly TextureSettings TextureMapping;

        public readonly int RandomSeed;
        public readonly int LengthSegmentsPerPanel;

        public BuildSettings(InfiniteCorrugatedRoof sheet)
        {
            PanelWidth = sheet.panelWidth;
            PanelLength = sheet.panelLength;
            VerticalOverlap = sheet.verticalOverlap;
            OverlapDistance = sheet.overlapDistance;
            Columns = sheet.columnCount;
            Rows = sheet.rowCount;

            WaveProfile = sheet.waveProfile;
            WaveCountPerPanel = sheet.wavesPerPanel;
            WaveAmplitude = sheet.corrugationAmplitude;
            Thickness = sheet.sheetThickness;
            LengthSegmentsPerPanel = sheet.lengthSegmentsPerPanel;

            PanelTransform = sheet.panelTransform;
            Noise = sheet.noise;
            Spline = sheet.spline;
            Cuts = sheet.cuts;
            TextureMapping = sheet.textureMapping;

            RandomSeed = sheet.randomSeed;
        }
    }
}