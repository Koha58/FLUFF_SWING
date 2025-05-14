using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// �v���C���[��Tilemap���Ground�^�C���ɑ΂��āA���C���[�iLineRenderer + DistanceJoint2D�j���g���Đڑ��E�X�C���O�ł���A�N�V�����𐧌䂷��X�N���v�g�B
/// �}�E�X���N���b�N�Őڑ��A�E�N���b�N�Ń��C���[��ؒf�A�j�I�u�W�F�N�g���ڕW�n�_�ֈړ����Ă��烏�C���[�𒣂�B
/// </summary>
public class WireActionScript : MonoBehaviour
{
    [Header("���C���[��[�Ɏg���j�I�u�W�F�N�g")]
    [SerializeField] private GameObject needle;

    private GameObject targetObject = null;

    // LineRenderer �R���|�[�l���g�i���C���[�̌����ڗp�j
    private LineRenderer lineRenderer => GetComponent<LineRenderer>();

    // DistanceJoint2D �R���|�[�l���g�i�����I�Ȑڑ��j
    private DistanceJoint2D distanceJoint => GetComponent<DistanceJoint2D>();

    // ���݂̐j�ړ��R���[�`��
    private Coroutine currentNeedleCoroutine;

    // === �萔�i�}�W�b�N�i���o�[�r���j ===

    // ���C���[�ڑ����A���ݒn�_����̋����Ɋ|����W���i90%�j
    private const float DISTANCE_ATTENUATION_RATIO = 0.9f; // �����Z�����Ĉ��萫���グ��

    // �����������̒l�������ƍĐڑ��𖳎�����i���ʂȐڑ��h�~�j
    private const float RECONNECT_DISTANCE_THRESHOLD = 0.3f;

    // �j�����B�����Ɣ��肷�邵�����l�i�ړI�n�Ƃ̋����j
    private const float NEEDLE_STOP_DISTANCE = 0.01f;

    // �j���ړ����鑬�x�i1�t���[��������j
    private const float NEEDLE_SPEED = 0.2f;

    // DistanceJoint2D �ɐݒ肷�鋗���̌W���i�����]�T���������ėh��₷���j
    private const float JOINT_DISTANCE_RATIO = 0.6f;

    // �v���C���[�ɗ^����X�C���O�J�n���̗�
    private const float SWING_FORCE = 300f;

    // Rigidbody2D �̏d�͔{���i�X�C���O�̋����ɉe���j
    private const float PLAYER_GRAVITY_SCALE = 3f;

    // �X�C���O���̌�����}����ݒ�
    private const float RIGIDBODY_LINEAR_DAMPING = 0f;
    private const float RIGIDBODY_ANGULAR_DAMPING = 0f;

    // LineRenderer�̒��_���i��Ɏn�_�ƏI�_��2�j
    private const int LINE_RENDERER_POINT_COUNT = 2;

    // �Œ胏�C���[�����i���C���[�̒��������ɕۂj
    private const float FIXED_WIRE_LENGTH = 5f; // ���C���[�̒�����5�ɌŒ�

    void Update()
    {
        HandleLeftClick();   // �ڑ�����
        HandleRightClick();  // �ؒf����
        UpdateLine();        // ���̌����ڍX�V
    }

    /// <summary>
    /// �}�E�X���N���b�N���̏����B�Ώۂ�Ground�^�C���ł���΃��C���[��ڑ��B
    /// </summary>
    private void HandleLeftClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        if (hit.collider == null) return;

        Tilemap tilemap = hit.collider.GetComponent<Tilemap>() ?? hit.collider.GetComponentInParent<Tilemap>();
        if (tilemap == null) return;

        Vector3Int cellPos = tilemap.WorldToCell(hit.point);
        TileBase tile = tilemap.GetTile(cellPos);

        if (tile is CustomTile customTile && customTile.tileType == CustomTile.TileType.Ground)
        {
            TryConnectWire(hit.point, hit.collider.gameObject);
        }
    }

    /// <summary>
    /// �}�E�X�E�N���b�N�Ń��C���[��ؒf����
    /// </summary>
    private void HandleRightClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            CutWire();
        }
    }

    /// <summary>
    /// LineRenderer�̈ʒu���v���C���[�Ɛڑ���ɍ��킹�čX�V
    /// </summary>
    private void UpdateLine()
    {
        if (distanceJoint.enabled && lineRenderer.positionCount >= LINE_RENDERER_POINT_COUNT)
        {
            lineRenderer.SetPosition(0, transform.position);                          // �v���C���[�̈ʒu
            lineRenderer.SetPosition(1, distanceJoint.connectedAnchor);              // �ڑ���
        }
    }

    /// <summary>
    /// ���C���[��ڑ����鏈���B��苗���ȏ㗣��Ă��Ȃ��ƍĐڑ����Ȃ��B
    /// </summary>
    private void TryConnectWire(Vector2 targetPos, GameObject hitObject)
    {
        // �Œ�̋�����ݒ肵�čĐڑ���h�~
        float newDistance = FIXED_WIRE_LENGTH;

        // �����̐ڑ������ƐV�����������߂�����ꍇ�͍Đڑ���h�~
        if (distanceJoint.enabled && distanceJoint.connectedAnchor != Vector2.zero)
        {
            float currentDistance = Vector2.Distance(transform.position, distanceJoint.connectedAnchor);
            if (Mathf.Abs(newDistance - currentDistance) < RECONNECT_DISTANCE_THRESHOLD)
            {
                Debug.Log("�������߂����ߍĐڑ����X�L�b�v");
                return;
            }
        }

        // �����̐j�R���[�`�����~�߂�
        if (currentNeedleCoroutine != null)
            StopCoroutine(currentNeedleCoroutine);

        // �V�����j�̔��ˏ����J�n
        currentNeedleCoroutine = StartCoroutine(ThrowNeedle(targetPos, hitObject));
    }

    /// <summary>
    /// ���C���[���������ALineRenderer���\���ɂ���B
    /// </summary>
    private void CutWire()
    {
        distanceJoint.enabled = false;
        lineRenderer.positionCount = 0;
        targetObject = null;
        Debug.Log("���C���[��ؒf���܂���");
    }

    /// <summary>
    /// �j��ړI�n�ɔ�΂��A���C���[�𒣂�R���[�`���B
    /// </summary>
    private IEnumerator ThrowNeedle(Vector2 targetPosition, GameObject hitObject)
    {
        // �j��ړI�n�Ɍ������Ĉړ�������
        while (Vector2.Distance(needle.transform.position, targetPosition) > NEEDLE_STOP_DISTANCE)
        {
            needle.transform.position = Vector2.MoveTowards(needle.transform.position, targetPosition, NEEDLE_SPEED);
            yield return null;
        }

        // �j���B��̏���
        needle.transform.position = targetPosition;
        targetObject = hitObject;

        DrawLine();

        // �Œ�̋�����ݒ�
        distanceJoint.distance = FIXED_WIRE_LENGTH;

        // �ő勗���݂̂𐧌�����ݒ��L����
        distanceJoint.maxDistanceOnly = true;
        distanceJoint.connectedAnchor = targetPosition;
        distanceJoint.connectedBody = null;
        distanceJoint.enabled = true;

        // �X�C���O�J�n���̏�����ݒ�
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = PLAYER_GRAVITY_SCALE;
        rb.linearDamping = RIGIDBODY_LINEAR_DAMPING;
        rb.angularDamping = RIGIDBODY_ANGULAR_DAMPING;

        // �ڑ��_�Ɍ������ĉ������ɏ�����^����i�h��n�߂̂��������j
        Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
        Vector2 tangent = new Vector2(-dir.y, dir.x); // �ڑ������ɐ����ȕ���
        rb.AddForce(tangent * SWING_FORCE);
    }

    /// <summary>
    /// LineRenderer �̐��������i�j���Ώۂɓ��B�����^�C�~���O�Łj
    /// </summary>
    private void DrawLine()
    {
        if (targetObject == null) return;

        lineRenderer.positionCount = LINE_RENDERER_POINT_COUNT;
        lineRenderer.SetPosition(0, transform.position);                  // �v���C���[�ʒu
        lineRenderer.SetPosition(1, targetObject.transform.position);     // �j�̈ʒu
    }

    /// <summary>
    /// �J�������l�����ă}�E�X�ʒu�����[���h���W�ɕϊ�
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }
}
