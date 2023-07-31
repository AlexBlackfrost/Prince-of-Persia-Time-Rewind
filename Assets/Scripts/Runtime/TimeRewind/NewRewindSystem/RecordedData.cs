public partial class TimeRewindController {
    private struct RecordedData {
        public object[] recordedVariables;
        public bool[] recordedMask;
        public float deltaTime;

        public RecordedData(object[] recordedVariables, bool[] recordedMask, float deltaTime) {
            this.recordedVariables = recordedVariables;
            this.recordedMask = recordedMask;
            this.deltaTime = deltaTime;
        }

        public bool HasRecordedDataFor(int id) {
            return recordedMask != null && recordedMask[id];
        }

        public object GetRecordedDataFor(int id) {
            int index = -1;
            for (int i = 0; i <= id; i++) {
                if (recordedMask[i]) {
                    index++;
                }
            }
            return recordedVariables[index];
        }
    }

}