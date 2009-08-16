using System;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Factories;

namespace Net.Brotherus
{
    public class AngleTargetLimitJoint
    {
        AngleJoint _angleJoint;
        AngleLimitJoint _limitJoint;

        public AngleTargetLimitJoint(PhysicsSimulator physicsSimulator, Body a, Body b, double minDegrees, double targetDegrees, double maxDegrees)
        {
            _angleJoint = JointFactory.Instance.CreateAngleJoint(physicsSimulator, a, b);
            _angleJoint.TargetAngle = MathExt.ToRadians(targetDegrees);
            _angleJoint.Softness = 0.5f;
            _angleJoint.MaxImpulse = 3000.0f;
            _limitJoint = JointFactory.Instance.CreateAngleLimitJoint(
                physicsSimulator, a, b, MathExt.ToRadians(minDegrees), MathExt.ToRadians(maxDegrees));
        }

        public float MaxImpulse { set { _angleJoint.MaxImpulse = value; } }

        public float TargetAngleRadians
        {
            get { return _angleJoint.TargetAngle; }
            set
            {
                if ((value > _limitJoint.LowerLimit) && (value < _limitJoint.UpperLimit))
                {
                    _angleJoint.TargetAngle = value;
                }
            }
        }

        public float TargetAngleDegrees { set { TargetAngleRadians = MathExt.ToRadians(value); } }
    }
}
