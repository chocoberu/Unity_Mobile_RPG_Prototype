// MonsterLove.StaeMachine�� �����Ͽ� �ۼ�
// ���� : https://github.com/thefuntastic/Unity3d-Finite-State-Machine

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class FSM<T> where T : System.Enum
{
    private T currentState;
    private T prevState;
    private MonoBehaviour ownerComponent;

    private Dictionary<T, Action> enterDelegates = new Dictionary<T, Action>();
    private Dictionary<T, Action> exitDelegates = new Dictionary<T, Action>();
    private Dictionary<T, Action> updateDelegates = new Dictionary<T, Action>();

    private bool isRunning = false;

    public FSM(MonoBehaviour component)
    {
        ownerComponent = component;

        // ownerComponent�� �޼ҵ� ����Ʈ�� �����ͼ� State�� ���õ� �Լ��� �����ϴ� ����
        MethodInfo[] methods = component.GetType().GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        for(int i = 0; i < methods.Length; i++)
        {
            ParseFunction(methods[i]);
        }
    }

    private bool ParseFunction(MethodInfo methodInfo)
    {
        T state = default(T);
        string name = methodInfo.Name;
        int index = name.IndexOf('_');

        // �Լ��� '_'�� ���� �Լ��� ����
        if (index < 0)
        {
            return false;
        }

        string stateName = name.Substring(0, index);
        string eventName = name.Substring(index + 1);

        // T�� StateName�� �����ϴ��� üũ
        try
        {
            state = (T)Enum.Parse(typeof(T), stateName);
        }
        catch (ArgumentException)
        {
            return false;
        }
        
        // �ش� Event Dictionary�� Add, Event�� Enter, Exit, Update�� ����
        if(true == eventName.Equals("Enter"))
        {
            Action action = (Action)Delegate.CreateDelegate(typeof(Action), ownerComponent, methodInfo);
            enterDelegates.Add(state, action);
        }
        else if(true == eventName.Equals("Exit"))
        {
            Action action = (Action)Delegate.CreateDelegate(typeof(Action), ownerComponent, methodInfo);
            exitDelegates.Add(state, action);
        }
        else if(true == eventName.Equals("Update"))
        {
            Action action = (Action)Delegate.CreateDelegate(typeof(Action), ownerComponent, methodInfo);
            updateDelegates.Add(state, action);
        }
        else
        {
            return false;
        }
       
        return true;
    }

    public void Transition(T nextState)
    {
        if (false == isRunning)
        {
            return;
        }

        Action currentExitAction;
        Action nextEnterAction;
        if (true == exitDelegates.TryGetValue(currentState, out currentExitAction))
        {
            currentExitAction.Invoke();
        }

        prevState = currentState;
        currentState = nextState;

        if(true == enterDelegates.TryGetValue(currentState, out nextEnterAction))
        {
            nextEnterAction.Invoke();
        }
    }

    public void OnUpdate()
    {
        if(false == isRunning)
        {
            return;
        }

        Action action;
        if(true == updateDelegates.TryGetValue(currentState, out action))
        {
            action.Invoke();
        }
    }

    public void StartFSM(T nextState)
    {
        if(true == isRunning)
        {
            return;
        }

        isRunning = true;
        Transition(nextState);
    }

    public void StopFSM()
    {
        isRunning = false;
    }

}
