using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace BezierSolution
{
	[AddComponentMenu( "Bezier Solution/Bezier Walker With Speed" )]
	[HelpURL( "https://github.com/yasirkula/UnityBezierSolution" )]
	public class BezierWalkerWithSpeed : BezierWalker
	{
		public BezierSpline spline;
		public GameObject boardComputer;
		public GameObject frontDisplay;
		public GameObject endFrontDisplay;
		public GameObject oldFrontDisplay;
		public Sprite newDisplay;
		public Sprite endDisplay;
		public GameObject[] lightsArray;
		public GameObject[] lightsArray2;

		public AudioSource ambientSound;
		public AudioSource announcement;
		public AudioSource endAnnouncement;

		public TravelMode travelMode;
		private int count = 0;

		public Animator animator;

		public BezierSpline[] mySplines;

		public float speed;
		[SerializeField]
		[Range( 0f, 1f )]
		private float m_normalizedT = 0f;

		public override BezierSpline Spline { get { return spline; } }

		public override float NormalizedT
		{
			get { return m_normalizedT; }
			set { m_normalizedT = value; }
		}

		IEnumerator switchSpline(){
			if(count < mySplines.Length){
				if(count == 1){
					yield return new WaitForSeconds(3f);
				} else if( count == 3){
					yield return new WaitForSeconds(3f);
				} else if( count == 2){
					yield return new WaitForSeconds(5f);
				}

				spline = mySplines[count];
				m_normalizedT = 0f;
				onPathCompletedCalledAt1 = false;
				onPathCompletedCalledAt0 = false;
				isGoingForward = true;
				

				// GO AROUND ACTIONS
				if(count == 1){
					yield return new WaitForSeconds(1f);

					// CHANGE DISPLAYS
					SpriteRenderer mySpriteRenderer = boardComputer.GetComponent<SpriteRenderer>();
					mySpriteRenderer.sprite = newDisplay;
					frontDisplay.SetActive(true);
					oldFrontDisplay.SetActive(false);

					//PLAY ANNOUNCEMENT
					announcement.Play();

					// TURN ALL THE LIGHTS ON
					foreach (GameObject lightObject in lightsArray)
					{
						Light lightComponent = lightObject.GetComponent<Light>();
						if (lightComponent != null)
						{
							lightComponent.enabled = true;
						}
					}
					// TURN ALL THE LIGHTS OFF
					foreach (GameObject lightObject in lightsArray2)
					{
						Light lightComponent2 = lightObject.GetComponent<Light>();
						if (lightComponent2 != null)
						{
							lightComponent2.enabled = false;
						}
					}
				}
				count++;
				if(count == 3){
					speed = 2f;
				} else if(count > 0){					
					speed = 3f;
				}
			}
			else{


				yield return new WaitForSeconds(2f);

				// TURN ALL THE LIGHTS ON
				foreach (GameObject lightObject in lightsArray)
				{
					Light lightComponent = lightObject.GetComponent<Light>();
					if (lightComponent != null)						
					{
						lightComponent.enabled = false;
					}
				}
				// TURN ALL THE LIGHTS OFF
				foreach (GameObject lightObject in lightsArray2)
				{
					Light lightComponent2 = lightObject.GetComponent<Light>();
					if (lightComponent2 != null)
					{
						lightComponent2.enabled = true;
					}
				}
				// CHANGE DISPLAYS
				frontDisplay.SetActive(false);
				endFrontDisplay.SetActive(true);
				SpriteRenderer mySpriteRenderer = boardComputer.GetComponent<SpriteRenderer>();
				mySpriteRenderer.sprite = endDisplay;

				vibrateStrength = 0f;

				//PLAY ANNOUNCEMENT
				endAnnouncement.Play();


				// TURN OFF MOTOR 
				ambientSound.volume = 0.2f;
				yield return new WaitForSeconds(2f);
				ambientSound.volume = 0.1f;
				yield return new WaitForSeconds(4f);
				ambientSound.volume = 0f;


			}
		}
		IEnumerator wait(){
			yield return new WaitForSeconds(4f);
			Execute(Time.deltaTime);
		}

		//public float movementLerpModifier = 10f;
		public float rotationLerpModifier = 10f;

		public LookAtMode lookAt = LookAtMode.Forward;

		private bool isGoingForward = true;
		public override bool MovingForward { get { return ( speed > 0f ) == isGoingForward; } }

		public UnityEvent onPathCompleted = new UnityEvent();
		private bool onPathCompletedCalledAt1 = false;
		private bool onPathCompletedCalledAt0 = false;

		public void Start()
		{
			foreach (GameObject lightObject in lightsArray)
				{
					Light lightComponent = lightObject.GetComponent<Light>();
					if (lightComponent != null)
					{
						lightComponent.enabled = false;
					}
					}
		}

		public float vibrateStrength;

		private void Update()
		{
			Execute( Time.deltaTime );
		}

		public override void Execute( float deltaTime )
		{

			 // randomly vibrate the object on all 3 dimensison, similar like an helicopter would
       		transform.position = new Vector3(transform.position.x + Random.Range(-vibrateStrength, vibrateStrength), transform.position.y + Random.Range(-vibrateStrength, vibrateStrength), transform.position.z + Random.Range(-vibrateStrength, vibrateStrength));
			
			float targetSpeed = ( isGoingForward ) ? speed : -speed;

			Vector3 targetPos = spline.MoveAlongSpline( ref m_normalizedT, targetSpeed * deltaTime );

			transform.position = targetPos;
			//transform.position = Vector3.Lerp( transform.position, targetPos, movementLerpModifier * deltaTime );

			bool movingForward = MovingForward;

			// pitch sound higher when drone initiates landing
			if(count == 0){
				if(m_normalizedT >= 0.98f){
					ambientSound.pitch = 1.2f;
					ambientSound.volume = 0.4f;
				} else if(m_normalizedT >= 0.95f){
					ambientSound.pitch = 1f;
					ambientSound.volume = 0.4f;
				} else if(m_normalizedT >= 0.89f){
					ambientSound.pitch = 0.8f;
					ambientSound.volume = 0.4f;
				}
			}

			// move drone slower when spline ends
			if( m_normalizedT >= 0.98f){
				speed = 0.5f;
			}
			else if( m_normalizedT >= 0.95f){
				speed = 1f;
			}
			else if( m_normalizedT >= 0.89f){
				speed = 3f;
			}

			// randomly vibrate the object on all 3 dimensison, similar like an helicopter would
			transform.position = new Vector3(transform.position.x + Random.Range(-vibrateStrength, vibrateStrength), transform.position.y + Random.Range(-vibrateStrength, vibrateStrength), transform.position.z + Random.Range(-vibrateStrength, vibrateStrength));
			if(count == 1 || count == 4){
				lookAt = LookAtMode.None;
			} else if (count == 3){
				lookAt = LookAtMode.Forward;
			}
			if( lookAt == LookAtMode.Forward )
			{
				BezierSpline.Segment segment = spline.GetSegmentAt( m_normalizedT );
				Quaternion targetRotation;
				float newXRotation = 0f; // Set the new x rotation value
				if( movingForward ){
					targetRotation = Quaternion.LookRotation( segment.GetTangent());
					targetRotation = Quaternion.Euler(newXRotation, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);}
				else{
					targetRotation = Quaternion.LookRotation( -segment.GetTangent());
					targetRotation = Quaternion.Euler(newXRotation, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);
				}
				transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, rotationLerpModifier * deltaTime );
			}
			else if( lookAt == LookAtMode.SplineExtraData )
				transform.rotation = Quaternion.Lerp( transform.rotation, spline.GetExtraData( m_normalizedT, extraDataLerpAsQuaternionFunction ), rotationLerpModifier * deltaTime );

			if( movingForward )
			{
				if( m_normalizedT >= 1f )
				{
					if( travelMode == TravelMode.Once )
						m_normalizedT = 1f;
					else if( travelMode == TravelMode.Loop )
						m_normalizedT -= 1f;
					else
					{
						m_normalizedT = 2f - m_normalizedT;
						isGoingForward = !isGoingForward;
					}

					if( !onPathCompletedCalledAt1 )
					{
						onPathCompletedCalledAt1 = true;


#if UNITY_EDITOR
						if( UnityEditor.EditorApplication.isPlaying )
#endif
							onPathCompleted.Invoke();
							 StartCoroutine(switchSpline());
					}
				}
				else
				{
					onPathCompletedCalledAt1 = false;
					
				}
			}
			else
			{
				if( m_normalizedT <= 0f )
				{
					if( travelMode == TravelMode.Once )
						m_normalizedT = 0f;
					else if( travelMode == TravelMode.Loop )
						m_normalizedT += 1f;
					else
					{
						m_normalizedT = -m_normalizedT;
						isGoingForward = !isGoingForward;
					}

					if( !onPathCompletedCalledAt0 )
					{
						onPathCompletedCalledAt0 = true;
#if UNITY_EDITOR
						if( UnityEditor.EditorApplication.isPlaying )
#endif
							onPathCompleted.Invoke();

							onPathCompletedCalledAt0 = false;
					}
				}
				else
				{
					onPathCompletedCalledAt0 = false;
				}
			}
		}
	}
}