using UnityEngine;

public class Creature : MonoBehaviour {

    private GameObject[] obstacles;
    private GA ga;
    public NeuralNetwork brain;

    private float lastSpeed = 0f;

    public Rigidbody2D body;
    public float baseSpeed, jumpForce;
    private bool grounded = true;
    private bool startSim;

	void Awake () {
        startSim = true;
        body = GetComponent<Rigidbody2D>();
        obstacles = new GameObject[4];
        obstacles[0] = GameObject.Find("Obstacle1");
        obstacles[1] = GameObject.Find("Obstacle2");
        obstacles[2] = GameObject.Find("Obstacle3");
        obstacles[3] = GameObject.Find("Obstacle4");
        ga = GetComponentInParent<GA>();
	}

    public void StartSimulation() {
        startSim = true;
    }

    void Update() {

        if (brain.initialized && startSim) {
            int obstacleID = NearestObstacleID();
            var obstaclePos = ObstaclePosition(obstacleID);
            // distance to nearest obstacle (normalized)
            float dist = (transform.position - obstaclePos).magnitude;
            var inputs = new float[] { dist, obstacleID / obstacles.Length };
            var outputs = brain.ComputeOutputs(inputs);
            lastSpeed = outputs[0];
            // distance travelled
            brain.genome.fitness = ((transform.position.x - ga.startPos.x) * 100);// * ((transform.position.x - ga.startPos.x) * 100);   
            Move(outputs[0]);
            if (outputs[1] > 0.5f && grounded)
                Jump();
        }

        if (transform.position.y < -4f) {
            gameObject.SetActive(false);
        }
    }

    private int NearestObstacleID() {
        if (transform.position.x > obstacles[2].transform.position.x)
            return 4;
        if (transform.position.x > obstacles[1].transform.position.x)
            return 3;
        else if (transform.position.x > obstacles[0].transform.position.x)
            return 2;
        return 1;
    }

    private Vector3 ObstaclePosition(int ID) {
        return obstacles[ID-1].transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision) {        
        if (collision.gameObject.tag.Equals("Obstacle")) {
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionStay2D(Collision2D collision) {

        if (collision.gameObject.tag.Equals("Ground")) {
            grounded = true;
        }
        else if (collision.gameObject.tag.Equals("Finish")) {
            Debug.LogError("End reached!");
            gameObject.name = "WINNER";
            ga.SaveBest();
            Debug.Break();
        }
    }

    private void Move(float speed) {
        body.velocity = new Vector2(baseSpeed * speed * Time.deltaTime, body.velocity.y);
    }

    private void Jump() {
        body.velocity = new Vector2(body.velocity.x, 0f);
        body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        grounded = false;
    }
}
