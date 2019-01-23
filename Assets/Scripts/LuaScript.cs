using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public class LuaScript {
    public Script m_script = null;
    public DynValue m_coroutine;
    private TextAsset m_luacodeTextAsset;

    public LuaScript(string file)
    {
        m_luacodeTextAsset = Resources.Load<TextAsset>(file);
        m_script = new Script();
        m_script.Options.DebugPrint = s => { Debug.Log(s); }; // if you want the Lua print function work, you should set up this.
    }

    public void Compile(bool isCoroutine = false)
    {
        DynValue function = m_script.DoString(m_luacodeTextAsset.text);
        if (isCoroutine)
        {
            m_coroutine = m_script.CreateCoroutine(function);
        }
    }
}
