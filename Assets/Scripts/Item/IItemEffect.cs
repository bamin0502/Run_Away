using UnityEngine;

public interface IItemEffect
{
    void ApplyEffect();
}

public class CoinEffect : IItemEffect
{
    public void ApplyEffect()
    {
        Debug.Log("코인 획득");
        //GameManager.Instance.AddCoin();
    }
}

public class JumpUpEffect : IItemEffect
{
    public void ApplyEffect()
    {
        Debug.Log("점프력 증가");
        
    }
}

public class MagnetEffect : IItemEffect
{
    public void ApplyEffect()
    {
        Debug.Log("자석 획득");
        
    }
}

public class FeverEffect : IItemEffect
{
    public void ApplyEffect()
    {
        Debug.Log("피버모드");
        
    }
}

