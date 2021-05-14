using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPackage : MonoBehaviour
{
    [SerializeField] Rigidbody player1;
    [SerializeField] Rigidbody player2;
    [SerializeField] GameObject handle1;
    [SerializeField] GameObject handle2;

    bool lifting = false;
    ConfigurableJoint confJoint1;
    ConfigurableJoint confJoint2;
    
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Space1();
        if (Input.GetKeyDown(KeyCode.G))
        {
            confJoint1.axis = Vector3.forward;
            print("##########");
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            confJoint1.axis = Vector3.forward;
            print("!!!!!!!!!");
        }
    }

    void FixedUpdate()
    {
        if (lifting) RotateToPackage();
    }

    void Space1()
    {
        lifting = true;
        transform.parent = null;
        rb.isKinematic = false;
        player2.isKinematic = false;
        confJoint1 = gameObject.AddComponent<ConfigurableJoint>();
        SetConfJoint(confJoint1, player1, 0);
        ConfigurableJoint confJoint12 = gameObject.AddComponent<ConfigurableJoint>();
        SetConfJoint(confJoint12, player1, 1);
        confJoint2 = gameObject.AddComponent<ConfigurableJoint>();
        SetConfJoint(confJoint2, player2, 0);
        ConfigurableJoint confJoint22 = gameObject.AddComponent<ConfigurableJoint>();
        SetConfJoint(confJoint22, player2, 1);
    }

    void Space3()
    {
        lifting = true;
        transform.parent = null;
        rb.isKinematic = false;
        player2.isKinematic = false;
        confJoint1 = player1.gameObject.AddComponent<ConfigurableJoint>();
        SetConfJoint2(confJoint1, player1);
        confJoint2 = player2.gameObject.AddComponent<ConfigurableJoint>();
        SetConfJoint2(confJoint2, player2);
    }

    // void Space2()
    // {
    //     lifting = true;
    //     transform.parent = null;
    //     CharacterJoint charJoint1 = gameObject.AddComponent<CharacterJoint>();
    //     SetCharJoint(charJoint1, player1);
    //     CharacterJoint charJoint2 = gameObject.AddComponent<CharacterJoint>();
    //     SetCharJoint(charJoint2, player2);
    //     rb.isKinematic = false;
    //     player2.isKinematic = false;
    // }

    void SetConfJoint(ConfigurableJoint confJoint, Rigidbody playerBody, int i)
    {
        Vector3 packagePos = transform.position;
        Vector3 target1 = new Vector3(packagePos.x, 0, packagePos.z);
        Vector3 current1 = new Vector3(playerBody.position.x, 0, playerBody.position.z);
        Quaternion rotation1 = Quaternion.LookRotation(target1 - current1);
        playerBody.transform.rotation = rotation1;

        confJoint.connectedBody = playerBody;
        print("Player pos: "+playerBody.position);
        Vector3 anchor = CalculateLocalAnchors(playerBody);
        print("Calc anchor: "+anchor);
        Vector3 handOffset = new Vector3(0.5f-i, 0, 0);
        Vector3 boxOffset = transform.InverseTransformVector(playerBody.transform.TransformVector(handOffset));
        print("Hand: "+handOffset);
        print("Box: "+boxOffset);
        confJoint.anchor = anchor+boxOffset;
        confJoint.axis = Vector3.zero;
        confJoint.autoConfigureConnectedAnchor = false;
        confJoint.connectedAnchor = new Vector3(0.5f-i, 0.5f, 0.7f);
        confJoint.xMotion = ConfigurableJointMotion.Locked;
        confJoint.yMotion = ConfigurableJointMotion.Limited;
        confJoint.zMotion = ConfigurableJointMotion.Locked;
        confJoint.angularXMotion = ConfigurableJointMotion.Locked;
        confJoint.angularYMotion = ConfigurableJointMotion.Free;
        confJoint.angularZMotion = ConfigurableJointMotion.Locked;
        float limit = 0.5f;
        SoftJointLimit sjl = new SoftJointLimit();
        sjl.limit = limit;
        confJoint.linearLimit = sjl;
    }

    void SetConfJoint2(ConfigurableJoint confJoint, Rigidbody playerBody)
    {
        confJoint.connectedBody = rb;
        confJoint.anchor = Vector3.forward;
        confJoint.axis = Vector3.zero;
        confJoint.autoConfigureConnectedAnchor = false;
        Vector3 anchor = CalculateLocalAnchors2(playerBody);
        print("anchor: "+anchor);
        confJoint.connectedAnchor = anchor;
        confJoint.xMotion = ConfigurableJointMotion.Locked;
        confJoint.yMotion = ConfigurableJointMotion.Limited;
        confJoint.zMotion = ConfigurableJointMotion.Locked;
        confJoint.angularXMotion = ConfigurableJointMotion.Locked;
        confJoint.angularYMotion = ConfigurableJointMotion.Free;
        confJoint.angularZMotion = ConfigurableJointMotion.Locked;
        float limit = 0.5f;
        SoftJointLimit sjl = new SoftJointLimit();
        sjl.limit = limit;
        confJoint.linearLimit = sjl;
    }

    // void SetCharJoint(CharacterJoint charJoint, Rigidbody conBody)
    // {
    //     charJoint.connectedBody = conBody;
    //     Vector3 anchor = CalculateLocalAnchors(conBody);
    //     print("anchor: "+anchor);
    //     charJoint.anchor = anchor;
    //     print("axis: "+charJoint.axis);
    //     charJoint.axis = (anchor.x == 0) ? new Vector3(1, 0, 0) : new Vector3(0, 0, 1);
    //     print("axisAfter: "+charJoint.axis);
    //     charJoint.autoConfigureConnectedAnchor = false;
    //     charJoint.connectedAnchor = Vector3.zero;
    // }

    Vector3 CalculateLocalAnchors(Rigidbody conBody)
    {
        float offset1 = 0.5f;
        Vector3[] list = {new Vector3(offset1, 0, 0), new Vector3(-offset1, 0, 0), new Vector3(0, 0, -offset1), new Vector3(0, 0, offset1)};

        Vector3 anchor = list[0];
        float min = Vector3.Distance(gameObject.transform.TransformPoint(list[0]), conBody.position);
        for (int i = 1; i < list.Length; i++)
        {
            Vector3 pos = gameObject.transform.TransformPoint(list[i]);
            float current = Vector3.Distance(pos, conBody.position);
            if (current < min) 
            {
                min = current;
                anchor = list[i];
            }
        }
        return anchor;
    }

    Vector3 CalculateLocalAnchors2(Rigidbody playerBody)
    {
        float offset1 = 0.5f;
        Vector3[] list = {new Vector3(offset1, 0, 0), new Vector3(-offset1, 0, 0), new Vector3(0, 0, offset1), new Vector3(0, 0, -offset1)};

        Vector3 anchor = list[0];
        float min = Vector3.Distance(gameObject.transform.TransformPoint(list[0]), playerBody.position);
        for (int i = 1; i < list.Length; i++)
        {
            Vector3 pos = gameObject.transform.TransformPoint(list[i]);
            float current = Vector3.Distance(pos, playerBody.position);
            if (current < min) 
            {
                min = current;
                anchor = list[i];
            }
        }
        return anchor;
    }

    void RotateToPackage()
    {
        Vector3 packagePos = transform.position;
        Vector3 target1 = new Vector3(packagePos.x, 0, packagePos.z);
        Vector3 current1 = new Vector3(player1.position.x, 0, player1.position.z);
        Quaternion rotation1 = Quaternion.LookRotation(target1 - current1);
        Vector3 target2 = new Vector3(packagePos.x, 0, packagePos.z);
        Vector3 current2 = new Vector3(player2.position.x, 0, player2.position.z);
        Quaternion rotation2 = Quaternion.LookRotation(target2 - current2);
        player1.MoveRotation(rotation1);
        player2.MoveRotation(rotation2);
    }
}
