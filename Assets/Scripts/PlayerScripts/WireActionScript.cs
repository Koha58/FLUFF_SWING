using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// �v���C���[�����C���[���g���Ēn�`�iTilemap�j�ɐڑ����A�X�C���O�ړ����s���A�N�V�����𐧌䂷��N���X�B
/// ���N���b�N�Őڑ��\�n�_���w�肵�A�j���΂��Đڑ��B
/// �E�N���b�N�Ń��C���[��ؒf�B
/// �ڑ����͌Œ蒷�̃��C���[�ŕ����I�ɐڑ�����A�ڑ�����ɃX�C���O�J�n�̗͂������B
/// </summary>
public class WireActionScript : MonoBehaviour
{
    [SerializeField] private GameObject needle;

    // �ڑ��Ώۂ̃I�u�W�F�N�g
    private GameObject targetObject = null;

    // ���C���[�̌����ڂ�S������ LineRenderer �R���|�[�l���g
    private LineRenderer lineRenderer => GetComponent<LineRenderer>();

    // �v���C���[��ڑ����镨���W���C���g�i�����Œ�j
    private DistanceJoint2D distanceJoint => GetComponent<DistanceJoint2D>();

    // ���ݐi�s���̐j�ړ��R���[�`���i���������ɓ������Ȃ����ߊǗ��j
    private Coroutine currentNeedleCoroutine;

    #region �萔
    private const float NEEDLE_STOP_DISTANCE = 0.01f;  // �j��~�̔��苗��
    private const float NEEDLE_SPEED = 0.2f;           // �j�̈ړ����x
    private const float SWING_FORCE = 300f;            // �X�C���O�J�n���ɉ������
    private const float PLAYER_GRAVITY_SCALE = 3f;     // �ڑ����̏d�̓X�P�[��
    private const float RIGIDBODY_LINEAR_DAMPING = 0f; // ��C��R
    private const float RIGIDBODY_ANGULAR_DAMPING = 0f;// ��]����
    private const int LINE_RENDERER_POINT_COUNT = 2;   // ���C���̓_��
    private const float FIXED_WIRE_LENGTH = 3.5f;      // ���C���[�̌Œ蒷��
    private const int LINE_START_INDEX = 0;            // ���C���̎n�_�C���f�b�N�X
    private const int LINE_END_INDEX = 1;              // ���C���̏I�_�C���f�b�N�X
    private const int LINE_POINT_NONE = 0;             // ���C����\�����̓_��
    #endregion

    void Update()
    {
        HandleLeftClick();   // ���N���b�N�F�ڑ�����
        HandleRightClick();  // �E�N���b�N�F�ؒf����
        UpdateLine();        // ��Ƀ��C���[�̌����ڂ��X�V
    }

    /// <summary>
    /// ���N���b�N���̐ڑ������B
    /// �N���b�N�ʒu�� Ground �^�C���ł���΁A���C���[��ڑ�����B
    /// </summary>
    private void HandleLeftClick()
    {
        // ���N���b�N��������Ă��Ȃ���Ή������Ȃ�
        if (!Input.GetMouseButtonDown(0)) return;

        // �}�E�X�̃��[���h���W���擾
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // �}�E�X���W��2D���C�L���X�g�i���̍��W�ɃI�u�W�F�N�g�����݂��邩�m�F�j
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        if (hit.collider == null) return; // �q�b�g���Ȃ���Ή������Ȃ�

        // �q�b�g�����I�u�W�F�N�g���� Tilemap ���擾�iTilemapCollider2D �̏ꍇ���z�肵�e���m�F�j
        Tilemap tilemap = hit.collider.GetComponent<Tilemap>() ?? hit.collider.GetComponentInParent<Tilemap>();
        if (tilemap == null) return; // Tilemap �łȂ���Ή������Ȃ�

        // �q�b�g�����ʒu�̃^�C�����W���擾
        Vector3Int cellPos = tilemap.WorldToCell(hit.point);

        // �Y���̃^�C�����擾
        TileBase tile = tilemap.GetTile(cellPos);

        // Ground �^�C�v�̃J�X�^���^�C���Ȃ�ڑ��������s��
        if (tile is CustomTile customTile && customTile.tileType == CustomTile.TileType.Ground)
        {
            TryConnectWire(hit.point, hit.collider.gameObject);
        }
    }

    /// <summary>
    /// �E�N���b�N���A���C���[��ؒf����B
    /// </summary>
    private void HandleRightClick()
    {
        // �E�N���b�N�������ꂽ�烏�C���[��ؒf
        if (Input.GetMouseButtonDown(1))
        {
            CutWire();
        }
    }

    /// <summary>
    /// ���C���[�̌����ځiLineRenderer�j���X�V����B
    /// </summary>
    private void UpdateLine()
    {
        // �W���C���g���L������ LineRenderer ���Œ���̓_���������Ă���ꍇ�̂ݍX�V
        if (distanceJoint.enabled && lineRenderer.positionCount >= LINE_RENDERER_POINT_COUNT)
        {
            // �n�_�̓v���C���[�i�����j
            lineRenderer.SetPosition(LINE_START_INDEX, transform.position);

            // �I�_�̓W���C���g�̐ڑ��A���J�[�i�ڑ����W�j
            lineRenderer.SetPosition(LINE_END_INDEX, distanceJoint.connectedAnchor);
        }
    }

    /// <summary>
    /// ���C���[�ڑ��v���B
    /// �����n�_�ɐڑ��ς݂̏ꍇ�͏������X�L�b�v�B
    /// �j���΂��R���[�`�����J�n�B
    /// </summary>
    private void TryConnectWire(Vector2 targetPos, GameObject hitObject)
    {
        // ���ɐڑ����Ȃ瓯���^�[�Q�b�g���m�F
        if (distanceJoint.enabled && distanceJoint.connectedAnchor != Vector2.zero)
        {
            bool isSameTarget = (Vector2.Distance(distanceJoint.connectedAnchor, targetPos) < 0.01f);
            if (isSameTarget)
            {
                // �����ꏊ�Ȃ�X�L�b�v�i���ʂȐڑ��������j
                Debug.Log("�����ꏊ�Ɋ��ɐڑ����̂��߃X�L�b�v");
                return;
            }
        }

        // �����̐j�R���[�`�����~�i���������N���h�~�j
        if (currentNeedleCoroutine != null)
            StopCoroutine(currentNeedleCoroutine);

        // �V�����j���΂��R���[�`�����J�n
        currentNeedleCoroutine = StartCoroutine(ThrowNeedle(targetPos, hitObject));
    }

    /// <summary>
    /// ���C���[��ؒf�B
    /// </summary>
    private void CutWire()
    {
        // �W���C���g�𖳌���
        distanceJoint.enabled = false;

        // ���C���[�̌����ڂ���\��
        lineRenderer.positionCount = LINE_POINT_NONE;

        // �ڑ��Ώۂ����Z�b�g
        targetObject = null;

        Debug.Log("���C���[��ؒf���܂���");
    }

    /// <summary>
    /// �j���^�[�Q�b�g�ʒu�܂ňړ����A���B�����烏�C���[�ڑ����s���R���[�`���B
    /// </summary>
    private IEnumerator ThrowNeedle(Vector2 targetPosition, GameObject hitObject)
    {
        // �v���C���[�ʒu
        Vector2 playerPosition = transform.position;

        // �v���C���[ �� �^�[�Q�b�g ����
        Vector2 directionToTarget = (targetPosition - playerPosition).normalized;

        // �v���C���[���猩�Ĕ��Ε����i�^�[�Q�b�g�����̋t�j
        Vector2 directionOpposite = -directionToTarget;

        // �j���v���C���[�ʒu�ɃZ�b�g
        needle.transform.position = playerPosition;

        // �j���^�[�Q�b�g�Ƃ͋t�����Ɉ�苗�������i���o�p�A�K�v�Ȃ�j
        float initialOffset = 0.5f; // �C�ӂ̋���
        needle.transform.position = playerPosition + directionOpposite * initialOffset;

        // �j���^�[�Q�b�g�����Ɉړ��i�����ڏ�͋t����^�[�Q�b�g�Ɍ������Ă���j
        while (Vector2.Distance(needle.transform.position, targetPosition) > NEEDLE_STOP_DISTANCE)
        {
            needle.transform.position = Vector2.MoveTowards(needle.transform.position, targetPosition, NEEDLE_SPEED);
            yield return null;
        }

        // �j���҂�����^�[�Q�b�g�ʒu�ɔz�u
        needle.transform.position = targetPosition;

        // ���Ƃ͏]���ʂ胏�C���[�ڑ�
        targetObject = hitObject;
        DrawLine();

        distanceJoint.enabled = false;
        distanceJoint.connectedBody = null;
        distanceJoint.connectedAnchor = targetPosition;
        distanceJoint.maxDistanceOnly = true;
        distanceJoint.distance = FIXED_WIRE_LENGTH;
        distanceJoint.enabled = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = PLAYER_GRAVITY_SCALE;
        rb.linearDamping = RIGIDBODY_LINEAR_DAMPING;
        rb.angularDamping = RIGIDBODY_ANGULAR_DAMPING;

        Vector2 dir = (targetPosition - playerPosition).normalized;
        Vector2 tangent = new Vector2(-dir.y, dir.x);
        rb.AddForce(tangent * SWING_FORCE);
    }


    /// <summary>
    /// ���C���[�̌����ڂ� LineRenderer �ŕ`��B
    /// </summary>
    private void DrawLine()
    {
        if (targetObject == null) return; // �ڑ��Ώۂ�������Ε`�悵�Ȃ�

        // LineRenderer �̓_�����Z�b�g
        lineRenderer.positionCount = LINE_RENDERER_POINT_COUNT;

        // �n�_�̓v���C���[
        lineRenderer.SetPosition(LINE_START_INDEX, transform.position);

        // �I�_�̓^�[�Q�b�g�I�u�W�F�N�g�̈ʒu
        lineRenderer.SetPosition(LINE_END_INDEX, targetObject.transform.position);
    }

    /// <summary>
    /// �}�E�X�ʒu�����[���h���W�Ŏ擾�B
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        // �}�E�X���W���X�N���[�����W����擾
        Vector3 mousePosition = Input.mousePosition;

        // �J�����̈ʒu�␳�i2D�J�����Ȃ̂�Z���𒲐��j
        mousePosition.z = -Camera.main.transform.position.z;

        // �X�N���[�����W���烏�[���h���W�ɕϊ�
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }
}
