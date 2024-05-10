using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCapacityBalancing.Models;

public class SprintSelectionShortTerm 
{
   
    public bool SelectByEndDate { get; set; }

   
    public bool SelectByNrSprints { get; set; }
    public int NumberofSprints { get; set; }

    public SprintSelectionShortTerm()
    {
        
    }

    public SprintSelectionShortTerm(bool selectByEndDate,bool selectByNrSprints, int numberofSprints)
    {
        SelectByEndDate = selectByEndDate;
        SelectByNrSprints = selectByNrSprints;
        NumberofSprints = numberofSprints;
    }
}
