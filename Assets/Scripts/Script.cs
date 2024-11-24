using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MyAgent : Agent
{
    //az eger sebessege
    public float speed = 5f;

    // a celobjektumra referencia
    public Transform TargetTransform;

    //diszkret action-ok enumja
    private enum ACTIONS
    {
        LEFT = 0,
        FORWARD = 1,
        RIGHT = 2,
        BACKWARD = 3
    }

    //minden epizod elejen meghivodik
    public override void OnEpisodeBegin()
    {
        //az eger es a sajt poziciojanak beallitasa
        transform.localPosition = new Vector3(7.5f, 0, -12.5f);

        TargetTransform.localPosition = new Vector3(-12, 0, 12);
    }

    //relevans infok atadasa az agensnek
    public override void CollectObservations(VectorSensor sensor)
    {
        //eger es sajt poziciok
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);

        sensor.AddObservation(TargetTransform.localPosition.x);
        sensor.AddObservation(TargetTransform.localPosition.z);

    }

    //agens altali action-re reagalva
    public override void OnActionReceived(ActionBuffers actions)
    {
        var actionTaken = actions.DiscreteActions[0];

        //az agenst az altala valasztott diszkret iranyba allitjuk 
        switch (actionTaken)
        {
            case (int)ACTIONS.FORWARD:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case (int)ACTIONS.LEFT:
                transform.rotation = Quaternion.Euler(0, -90, 0);
                break;
            case (int)ACTIONS.RIGHT:
                transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
            case (int)ACTIONS.BACKWARD:
                transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
        }

        //az agens elore mozditasa
        transform.Translate(Vector3.forward * speed * Time.fixedDeltaTime);

        //az agens es a celobjektum kozotti tavolsag
        float distanceToTarget = Vector3.Distance(transform.localPosition, TargetTransform.localPosition);

        //az agenst a tavolsag fuggvenyeben normalizalt negativ jutalommal buntetjuk
        float reward = -distanceToTarget/100; 
        AddReward(reward);

        // agens osztonzese, hogy hamarabb talalja meg a celt
        AddReward(-0.01f);
        

    }


    //agens debugolasahoz Heuristic fuggv.
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.DiscreteActions;

        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");

        if (horizontal == -1)
        {
            actions[0] = (int)ACTIONS.LEFT;
        }
        else if (horizontal == +1)
        {
            actions[0] = (int)ACTIONS.RIGHT;
        }
        else if (vertical == -1)
        {
            actions[0] = (int)ACTIONS.BACKWARD;
        }
        else if (vertical == +1)
        {
            actions[0] = (int)ACTIONS.FORWARD;
        }
        else
        {
            actions[0] = (int)ACTIONS.FORWARD;
        }
    }

    //agens barmely objektummal valo utkozesere reagalva
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Cube")
        {
            //viszonylyag nagy negativ jutalmat kap, annak osztonzesere, hogy ne menjen a falnak
            AddReward(-10);
            EndEpisode();
        }
        else if (collision.collider.tag == "Cheese")
        {
            //nagy reward az exploitation elkerulese vegett (mivel megtortent:D)
            AddReward(+10);
            EndEpisode();
        }
    }

}
