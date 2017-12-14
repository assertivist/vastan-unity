using System.Collections;
using System;

public class FixedMath{}

public struct Fixed
{
    private static int DECIMAL_SHIFT = 16;

    public Int64 raw_value; //raw Q16 value

    public Int16 int_value
    {
        get
        {
            return (Int16)((raw_value >> DECIMAL_SHIFT) + (raw_value < 0 ? 1 : 0));
        }
    }
    public Int16 raw_decimal_value
    {
        get
        {
            return (Int16)(raw_value & 0xffff);
        }
    }
    public float decimal_value
    {
        get
        {
            return raw_decimal_value / (float)(1 << DECIMAL_SHIFT);
        }
    }
    public float float_value
    {
        get
        {
            return (int)(raw_value >> DECIMAL_SHIFT) + (decimal_value);
        }
    }
    public static Fixed FromWholeInt(int value)
    {
        Fixed fix = new Fixed();
        fix.raw_value = (value << DECIMAL_SHIFT);
        return fix;
    }
    public static Fixed FromFloat(float value)
    {
        int whole = (int)System.Math.Truncate(value);
        int dec = (int)(System.Math.Abs(value - whole) * (1 << DECIMAL_SHIFT));

        if (whole < 0)
            whole--;

        Fixed fix = new Fixed();
        fix.raw_value = (whole << 16) | (dec & 0xffff);
        return fix;
    }

    public Fixed(int in_raw_value)
    {
        raw_value = in_raw_value;
    }

    public static explicit operator Fixed(int whole)
    {
        return Fixed.FromWholeInt(whole);
    }

    public static explicit operator int(Fixed fix)
    {
        return fix.int_value;
    }

    public static explicit operator Fixed(float float_val)
    {
        return Fixed.FromFloat(float_val);
    }

    public static explicit operator float(Fixed fix_int)
    {
        return fix_int.float_value;
    }

    public static Fixed operator +(Fixed lhs, Fixed rhs)
    {
        lhs.raw_value += rhs.raw_value;
        return lhs;
    }

    public static Fixed operator -(Fixed lhs, Fixed rhs)
    {
        lhs.raw_value -= rhs.raw_value;
        return lhs;
    }

    public static Fixed operator *(Fixed lhs, Fixed rhs)
    {
        lhs.raw_value *= rhs.raw_value;
        lhs.raw_value >>= DECIMAL_SHIFT; // Q32 -> Q16
        return lhs;
    }

    public static Fixed operator /(Fixed lhs, Fixed rhs)
    {
        lhs.raw_value <<= DECIMAL_SHIFT; // Q16 -> Q32
        lhs.raw_value /= rhs.raw_value; // Q32 -> Q16
        return lhs;
    }
}





