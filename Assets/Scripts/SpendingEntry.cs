using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpendingEntry
{
    public DateTime date;
    public float amount;
    public float additionalAmount; // For Electricity_Spending (e.g., Solar_kWh)

    public SpendingEntry() { }

    public SpendingEntry(DateTime date, float amount, float additionalAmount = 0f)
    {
        this.date = date;
        this.amount = amount;
        this.additionalAmount = additionalAmount;
    }
}
