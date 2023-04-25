using CoreProductionModel.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreProductionModel.Models
{
    public class DefaultSign : Sign
    {
        private static readonly Dictionary<string, SignData> _buffer = new Dictionary<string, SignData>();
        private static int _lastToken = int.MinValue;

        private readonly int _token;
        public override int Token => _token;

        private DefaultSign(int token) => _token = token;

        public override string GetValue() => _buffer.First(x => x.Value.ReferenceObjectSign.Token == Token).Value.OriginalValue;

        public static DefaultSign Create(string signValue)
        {
            if(string.IsNullOrWhiteSpace(signValue)) throw new ArgumentNullException(nameof(signValue));
            string originalSign = signValue.Trim();
            signValue = originalSign.ToLower();
            if (_buffer.TryGetValue(signValue, out SignData signData)) return signData.ReferenceObjectSign;
            signData = new SignData(originalSign, new DefaultSign(_lastToken++));
            _buffer.Add(signValue, signData);
            return signData.ReferenceObjectSign;
        }

        public override string ToString() => GetValue();

        public static implicit operator DefaultSign(string signValue) => Create(signValue);
        private struct SignData
        {
            public string OriginalValue { get; private set; }
            public DefaultSign ReferenceObjectSign { get; private set; }

            public SignData(string originalValue, DefaultSign referenceObjectSign)
            {
                OriginalValue=originalValue??throw new ArgumentNullException(nameof(originalValue));
                ReferenceObjectSign=referenceObjectSign??throw new ArgumentNullException(nameof(referenceObjectSign));
            }
        }
    }
}
