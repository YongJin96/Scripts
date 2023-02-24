using UnityEngine;

public class Spring
{
    private float Strength;
    private float Damper;
    private float Target;
    private float Velocity;
    private float value;

    public void Update(float _deltaTime)
    {
        var direction = Target - value >= 0 ? 1f : -1f;
        var force = Mathf.Abs(Target - value) * Strength;
        Velocity += (force * direction - Velocity * Damper) * _deltaTime;
        value += Velocity * _deltaTime;
    }

    public void Reset()
    {
        Velocity = 0f;
        value = 0f;
    }

    public void SetValue(float _value)
    {
        this.value = _value;
    }

    public void SetTarget(float _target)
    {
        this.Target = _target;
    }

    public void SetDamper(float _damper)
    {
        this.Damper = _damper;
    }

    public void SetStrength(float _strength)
    {
        this.Strength = _strength;
    }

    public void SetVelocity(float _velocity)
    {
        this.Velocity = _velocity;
    }

    public float Value => value;
}
