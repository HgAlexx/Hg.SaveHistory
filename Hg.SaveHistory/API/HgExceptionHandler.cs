using System;
using NLua;

namespace Hg.SaveHistory.API
{
    // Put all flat functions here

    public static class HgExceptionHandler
    {
        #region Members

        public static object[] TryCatchFinally(LuaFunction tryCode, LuaFunction catchCode, LuaFunction finallyCode)
        {
            try
            {
                return tryCode.Call();
            }
            catch (Exception exception)
            {
                return catchCode.Call(exception);
            }
            finally
            {
                finallyCode.Call();
            }
        }

        #endregion
    }
}