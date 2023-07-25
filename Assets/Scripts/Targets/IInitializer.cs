using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInitializer
{
    public void Initialize(BaseTarget target);
    public void Initialize(BaseObstacle type);
}
