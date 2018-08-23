using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[System.Serializable]
public struct Division {

    public int numberOfDivisions { get; set; }
    public float maxPercent { get; set; }
    public float minPercent { get; set; }

    public Division(int numberOfDivisions, float minPercent, float maxPercent) {
        this.numberOfDivisions = numberOfDivisions;
        this.maxPercent = maxPercent;
        this.minPercent = minPercent;
    }


}