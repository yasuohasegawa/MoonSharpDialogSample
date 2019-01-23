using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using MoonSharp.Interpreter;

public enum eDialogState
{
    Mess = 0,
    Select = 1,
    End = 2
}

[MoonSharpUserData]
class Dialog
{
    public void ShowMessage(string mess)
    {
        Debug.Log(mess);
        Main.GetInstance().SetMess(mess);
    }

    // Do whatever you want.
    public void ShowSelect(string mess, params string[] selects)
    {
        Debug.Log(mess);
        Main.GetInstance().SetMess(mess);
    }
}

public class Main : MonoBehaviour {

    private const string LUA_TEST_PATH = "Lua/Common";
    private const string LUA_DIALOG_PATH = "Lua/Dialog";

    private LuaScript testLua;
    private LuaScript dialogLua;

    [SerializeField]
    private Text m_lb;


    static private Main instance = null;
    static public Main GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start () {
        // Register C# Class into Lua
        UserData.RegisterAssembly(typeof(Dialog).Assembly);
        TestLuaScript();
    }

    private void TestLuaScript()
    {
        testLua = new LuaScript(LUA_TEST_PATH);
        dialogLua = new LuaScript(LUA_DIALOG_PATH);

        // setup Unity callbacks
        testLua.m_script.Globals["CallbackTest"] = (Func<int>)CallbackTest;
        testLua.m_script.Globals["CallbackTestWithArguments"] = (Func<int, int, int>)CallbackTestWithArguments;

        // compile
        testLua.Compile();


        // Getting number value
        var res = testLua.m_script.Call(testLua.m_script.Globals["testNumber"], 1, 2);
        Debug.Log(res.Number);

        // Getting string value
        res = testLua.m_script.Call(testLua.m_script.Globals["testString"]);
        Debug.Log(res.String);

        // Getting table values
        res = testLua.m_script.Call(testLua.m_script.Globals["testTable"]);
        foreach (var val in res.Table.Values)
        {
            Debug.Log(val);
        }

        foreach (var key in res.Table.Keys)
        {
            Debug.Log(key);
        }

        // Update C# table values from Lua
        List<float> dict = new List<float>();
        dict.Add(10.0f);
        dict.Add(20.0f);
        dict.Add(30.0f);
        res = testLua.m_script.Call(testLua.m_script.Globals["updateList"], dict);
        foreach (var val in res.Table.Values)
        {
            Debug.Log(val);
        }

        // boolean test
        res = testLua.m_script.Call(testLua.m_script.Globals["testBool"]);
        Debug.Log(res);
        res = testLua.m_script.Call(testLua.m_script.Globals["testBool"]);
        Debug.Log(res);
        res = testLua.m_script.Call(testLua.m_script.Globals["testBool"]);
        Debug.Log(res);

        // callback test
        testLua.m_script.Call(testLua.m_script.Globals["testCsharpCallBack"]);

        // tupple test
        res = testLua.m_script.Call(testLua.m_script.Globals["testTuple"]);
        Debug.Log(res.Tuple[0].Number + "," + res.Tuple[1].String);

        // call static class method
        res = testLua.m_script.Call(testLua.m_script.Globals["testClassMethod"]);


        //Lua/Dialog
        dialogLua.m_script.Globals["dialog"] = new Dialog();
        dialogLua.Compile(true);
        dialogLua.m_coroutine.Coroutine.Resume();
    }

    private int CallbackTest()
    {
        Debug.Log("from Lua: CallbackTest");
        return 0;
    }

    private int CallbackTestWithArguments(int a, int b)
    {
        Debug.Log("from Lua: " + a + "," + b);
        return 0;
    }

    private void UpdateDialog()
    {
        Debug.Log(dialogLua.m_coroutine.Coroutine.State);
        if (dialogLua.m_coroutine.Coroutine.State != CoroutineState.Dead)
        {
            var res = dialogLua.m_script.Call(dialogLua.m_script.Globals["GetDialogState"]);
            Debug.Log(res.Number);
            if (res.Number == (int)eDialogState.Mess)
            {
                dialogLua.m_coroutine.Coroutine.Resume();
            }
            else if (res.Number == (int)eDialogState.Select)
            {
                dialogLua.m_coroutine.Coroutine.Resume();
                int a = (int)UnityEngine.Random.Range(0f, 2f);
                Debug.Log("select:"+a);
                dialogLua.m_coroutine.Coroutine.Resume(a);
            }
        }
    }

    public void SetMess(string mess)
    {
        m_lb.text = mess;
    }

    public void OnNext()
    {
        UpdateDialog();
    }
}
