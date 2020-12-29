using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleculeController : MonoBehaviour
{
    public static MoleculeController instance;

    public List<Transform> movingTransforms = new List<Transform>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }

    public void MoveTowards(Transform _object, Transform from, Transform to, float progress)
    {
        if (!movingTransforms.Contains(_object))
        {
            Vector3 direction = to.position - from.position;

            _object.position = from.position + (direction * progress);
        }
    }

    public IEnumerator MoveToPosition(Transform _object, Transform from, Transform to, float delay)
    {
        if (!movingTransforms.Contains(_object))
        {
            movingTransforms.Add(_object);

            float elapsedTime = 0;

            while (movingTransforms.Contains(_object))
            {
                elapsedTime += Time.deltaTime;

                _object.transform.position = Vector3.Lerp(from.position, to.position, elapsedTime / delay);

                if (elapsedTime >= delay)
                {
                    movingTransforms.Remove(_object);
                    break;
                }

                yield return null;
            }
        }
        yield return null;
    }

    public IEnumerator MoveToPosition(Transform _object, Vector3 from, Transform to, float delay)
    {
        if (!movingTransforms.Contains(_object))
        {
            movingTransforms.Add(_object);

            float elapsedTime = 0;

            while (movingTransforms.Contains(_object))
            {
                elapsedTime += Time.deltaTime;

                _object.transform.position = Vector3.Lerp(from, to.position, elapsedTime / delay);

                if (elapsedTime >= delay)
                {
                    movingTransforms.Remove(_object);

                    
                    break;
                }

                yield return null;
            }
        }
        yield return null;
    }
    public IEnumerator MoveToPosition(Transform _object, Transform from, Vector3 to, float delay)
    {
        if (!movingTransforms.Contains(_object))
        {
            movingTransforms.Add(_object);

            float elapsedTime = 0;

            while (movingTransforms.Contains(_object))
            {
                elapsedTime += Time.deltaTime;

                _object.transform.position = Vector3.Lerp(from.position, to, elapsedTime / delay);

                if (elapsedTime >= delay)
                {
                    movingTransforms.Remove(_object);
                    break;
                }

                yield return null;
            }
        }
        yield return null;
    }

    public IEnumerator MoveBetween(Transform _object, Transform from, Transform to, float moveTime)
    {
        if (!movingTransforms.Contains(_object))
        {
            movingTransforms.Add(_object);

            float elapsedTime = 0;

            bool direction = true;

            while (movingTransforms.Contains(_object))
            {
                elapsedTime += Time.deltaTime;

                if (direction) _object.transform.position = Vector3.Lerp(from.position, to.position, elapsedTime / moveTime);
                else _object.transform.position = Vector3.Lerp(to.position, from.position, elapsedTime / moveTime);

                if (elapsedTime >= moveTime)
                {
                    elapsedTime = 0;
                    direction = !direction;
                }

                yield return null;
            }
        }
        yield return null;
    }

    public IEnumerator FlashColor(Renderer renderer, Color fromColor, Color toColor, float frequency)
    {
        float timer = 0;
        float direction = 1;

        while (true)
        {
            if (timer >= 1) direction = -1;
            else if (timer <= 0) direction = 1;

            timer += Time.deltaTime / frequency * direction;

            renderer.material.color = Color.Lerp(fromColor, toColor, timer);

            yield return null;
        }
    }

    public IEnumerator FlashColor(Renderer renderer, Color fromColor, Color toColor, float frequency, float duration)
    {
        float timer1 = 0;
        float timer2 = 0;
        float direction = 1;

        while (true)
        {
            if (timer1 >= 1) direction = -1;
            else if (timer1 <= 0) direction = 1;

            timer1 += Time.deltaTime / frequency * direction;
            timer2 += Time.deltaTime;

            renderer.material.color = Color.Lerp(fromColor, toColor, timer1 );

            if(timer2 >= duration)
            {
                break;
            }

            yield return null;
        }
    }
}
