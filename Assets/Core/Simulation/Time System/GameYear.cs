public enum Era { BC, AD }

[System.Serializable]
public struct GameYear
{
    public int year; // Always positive
    public Era era;

    public void AdvanceYear()
    {
        if (era == Era.BC)
        {
            year--;
            if (year == 0) // There is no year 0 historically
            {
                year = 1;
                era = Era.AD;
            }
        }
        else
        {
            year++;
        }
    }

    public override string ToString()
    {
        return $"{year} {era}";
    }
}
