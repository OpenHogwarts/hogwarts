using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MathUtils
{
	public static float GetQuatLength(Quaternion q)
	{
		return Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
	}

	public static Quaternion GetQuatConjugate(Quaternion q)
	{
		return new Quaternion(-q.x, -q.y, -q.z, q.w);
	}

	/// <summary>
	/// Logarithm of a unit quaternion. The result is not necessary a unit quaternion.
	/// </summary>
	public static Quaternion GetQuatLog(Quaternion q)
	{
		Quaternion res = q;
		res.w = 0;

		if (Mathf.Abs(q.w) < 1.0f)
		{
			float theta = Mathf.Acos(q.w);
			float sin_theta = Mathf.Sin(theta);

			if (Mathf.Abs(sin_theta) > 0.0001)
			{
				float coef = theta / sin_theta;
				res.x = q.x * coef;
				res.y = q.y * coef;
				res.z = q.z * coef;
			}
		}

		return res;
	}

	public static Quaternion GetQuatExp(Quaternion q)
	{
		Quaternion res = q;

		float fAngle = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z);
		float fSin = Mathf.Sin(fAngle);

		res.w = Mathf.Cos(fAngle);

		if (Mathf.Abs(fSin) > 0.0001)
		{
			float coef = fSin / fAngle;
			res.x = coef * q.x;
			res.y = coef * q.y;
			res.z = coef * q.z;
		}

		return res;
	}

	/// <summary>
	/// SQUAD Spherical Quadrangle interpolation [Shoe87]
	/// </summary>
	public static Quaternion GetQuatSquad(float t, Quaternion q0, Quaternion q1, Quaternion a0, Quaternion a1)
	{
		float slerpT = 2.0f * t * (1.0f - t);

		Quaternion slerpP = Slerp(q0, q1, t);
		Quaternion slerpQ = Slerp(a0, a1, t);

		return Slerp(slerpP, slerpQ, slerpT);
	}

	public static Quaternion GetSquadIntermediate(Quaternion q0, Quaternion q1, Quaternion q2)
	{
		Quaternion q1Inv = GetQuatConjugate(q1);
		Quaternion p0 = GetQuatLog(q1Inv * q0);
		Quaternion p2 = GetQuatLog(q1Inv * q2);
		Quaternion sum = new Quaternion(-0.25f * (p0.x + p2.x), -0.25f * (p0.y + p2.y), -0.25f * (p0.z + p2.z), -0.25f * (p0.w + p2.w));

		return q1 * GetQuatExp(sum);
	}

	/// <summary>
	/// Smooths the input parameter t.
	/// If less than k1 ir greater than k2, it uses a sin.
	/// Between k1 and k2 it uses linear interp.
	/// </summary>
	public static float Ease(float t, float k1, float k2)
	{
		float f; float s;

		f = k1 * 2 / Mathf.PI + k2 - k1 + (1.0f - k2) * 2 / Mathf.PI;

		if (t < k1)
		{
			s = k1 * (2 / Mathf.PI) * (Mathf.Sin((t / k1) * Mathf.PI / 2 - Mathf.PI / 2) + 1);
		}
		else
			if (t < k2)
			{
				s = (2 * k1 / Mathf.PI + t - k1);
			}
			else
			{
				s = 2 * k1 / Mathf.PI + k2 - k1 + ((1 - k2) * (2 / Mathf.PI)) * Mathf.Sin(((t - k2) / (1.0f - k2)) * Mathf.PI / 2);
			}

		return (s / f);
	}

	/// <summary>
	/// We need this because Quaternion.Slerp always uses the shortest arc.
	/// </summary>
	public static Quaternion Slerp(Quaternion p, Quaternion q, float t)
	{
		Quaternion ret;

		float fCos = Quaternion.Dot(p, q);

		if ((1.0f + fCos) > 0.00001)
		{
			float fCoeff0, fCoeff1;

			if ((1.0f - fCos) > 0.00001)
			{
				float omega = Mathf.Acos(fCos);
				float invSin = 1.0f / Mathf.Sin(omega);
				fCoeff0 = Mathf.Sin((1.0f - t) * omega) * invSin;
				fCoeff1 = Mathf.Sin(t * omega) * invSin;
			}
			else
			{
				fCoeff0 = 1.0f - t;
				fCoeff1 = t;
			}

			ret.x = fCoeff0 * p.x + fCoeff1 * q.x;
			ret.y = fCoeff0 * p.y + fCoeff1 * q.y;
			ret.z = fCoeff0 * p.z + fCoeff1 * q.z;
			ret.w = fCoeff0 * p.w + fCoeff1 * q.w;
		}
		else
		{
			float fCoeff0 = Mathf.Sin((1.0f - t) * Mathf.PI * 0.5f);
			float fCoeff1 = Mathf.Sin(t * Mathf.PI * 0.5f);

			ret.x = fCoeff0 * p.x - fCoeff1 * p.y;
			ret.y = fCoeff0 * p.y + fCoeff1 * p.x;
			ret.z = fCoeff0 * p.z - fCoeff1 * p.w;
			ret.w = p.z;
		}

		return ret;
	}
}