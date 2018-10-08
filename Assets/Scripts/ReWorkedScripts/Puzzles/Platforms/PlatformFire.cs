﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformFire : Platform
{

    [SerializeField]
    PlatformFirePropulsor[] propulsors;

    public float platformSpeed;
    public float lerpValue;

    public float positionOffset;
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;
    
    Vector3 directionedSpeed;

    bool isMoving;


    void Start ()
    {
        var p = GetComponentsInChildren<PlatformFirePropulsor>();
        propulsors = new PlatformFirePropulsor[p.Length];
        propulsors = p;

        UpdatesManager.instance.AddUpdate(UpdateType.UPDATE, Execute);
	}
	
	void Execute()
    {
        for (int i = 0; i < propulsors.Length; i++)
        {
            if (propulsors[i].isOnFire)
            {
                switch (propulsors[i].dir)
                {
                    case PlatformFirePropulsor.direction.X:
                        if(transform.position.x <= maxX - positionOffset)
                        {
                            directionedSpeed.x = Mathf.Lerp(directionedSpeed.x, platformSpeed, lerpValue);
                        }else if(transform.position.x < maxX)
                        {
                            directionedSpeed.x = Mathf.Lerp(directionedSpeed.x, 0, lerpValue);
                        }else
                        {
                            directionedSpeed.x = 0;
                        }
                        break;
                    case PlatformFirePropulsor.direction.X_NEGATIVE:
                        if(transform.position.x >= minX + positionOffset)
                        {
                            directionedSpeed.x = Mathf.Lerp(directionedSpeed.x, -platformSpeed, lerpValue);
                        }else if(transform.position.x > minX)
                        {
                            directionedSpeed.x = Mathf.Lerp(directionedSpeed.x,0,lerpValue);
                        }else
                        {
                           directionedSpeed.x = 0; 
                        }
                        break;
                    case PlatformFirePropulsor.direction.Z:
                        if(transform.position.z <= maxZ - positionOffset)
                        {
                            directionedSpeed.z = Mathf.Lerp(directionedSpeed.z, platformSpeed, lerpValue);

                        }else if(transform.position.z <= maxZ)
                        {
                            directionedSpeed.z = Mathf.Lerp(directionedSpeed.z,0,lerpValue);
                        }else
                        {
                            directionedSpeed.z = 0;
                        }
                        break;
                    case PlatformFirePropulsor.direction.Z_NEGATIVE:
                        if(transform.position.z >= minZ + positionOffset)
                        {
                            directionedSpeed.z = Mathf.Lerp(directionedSpeed.z, -platformSpeed, lerpValue);
                        }else if(transform.position.z >= minZ)
                        {
                            directionedSpeed.z = Mathf.Lerp(directionedSpeed.z,0,lerpValue);
                        }else
                        {
                            directionedSpeed.z = 0;
                        }
                        
                        break;
                    default:
                        break;
                }
                isMoving = true;
                propulsors[i].isOnFire = false;
            }
        }
        if (!isMoving)
        {
            directionedSpeed = Vector3.Lerp(directionedSpeed, Vector3.zero, lerpValue);
            
        }
        transform.position += directionedSpeed * Time.deltaTime;
        isMoving = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        var p1 = new Vector3(minX, transform.position.y, minZ);
        var p2 = new Vector3(minX, transform.position.y, maxZ);
        var p3 = new Vector3(maxX, transform.position.y, minZ);
        var p4 = new Vector3(maxX, transform.position.y, maxZ);

        Gizmos.DrawLine(p1,p2);
        Gizmos.DrawLine(p2,p4);
        Gizmos.DrawLine(p4,p3);
        Gizmos.DrawLine(p3,p1);
    }

    private void OnDestroy()
    {
        UpdatesManager.instance.RemoveUpdate(UpdateType.UPDATE, Execute);
    }
}
