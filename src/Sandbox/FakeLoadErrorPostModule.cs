namespace Elmah.Sandbox
{
    #region Imports

    using System;

    #endregion

    public class FakeLoadErrorPostModule : ErrorPostModule
    {
        protected override string GetHandshakeToken()
        {
            // UNDOCUMENTED HACK (for testing purposes only!!): if thr handshakeToken
            // contains a comma-separated list of tokens, the module will pick one
            // of them randomly, to simulate different sources generating errors.

            var tokens = base.GetHandshakeToken().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            var random = new Random();

            return tokens[random.Next(tokens.Length)];
        }
    }
}
