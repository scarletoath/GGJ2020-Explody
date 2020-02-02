using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Explodable : MonoBehaviour
{
    public System.Action<List<GameObject>> OnFragmentsGenerated;

    public bool allowRuntimeFragmentation = false;
    public int extraPoints = 0;
    public int subshatterSteps = 0;

    public string fragmentLayer = "Default";
    public string sortingLayerName = "Default";
    public int orderInLayer = 0;

    public enum ShatterType
    {
        Triangle,
        Voronoi
    };

    public ShatterType shatterType;
    public List<GameObject> fragments = new List<GameObject>();
	public List <(Vector3 pos , Quaternion rot)> fragmentTxs = new List <(Vector3 pos , Quaternion rot)> ();
    private List<List<Vector2>> polygons = new List<List<Vector2>>();

    /// <summary>
    /// Creates fragments if necessary and destroys original gameobject
    /// </summary>
    public void explode()
    {
        //if fragments were not created before runtime then create them now
        if (fragments.Count == 0 && allowRuntimeFragmentation)
        {
            generateFragments();
			foreach ( GameObject frag in fragments )
			{
				frag.transform.parent = transform;
			}
		}
		//otherwise activate them
		else
        {
			foreach ( GameObject frag in fragments)
			{
				var mRend = frag.GetComponent<MeshRenderer>();
                if (mRend.sharedMaterial == null)
                {
                    var sRend = GetComponent<SpriteRenderer>();
                    mRend.sharedMaterial = sRend.sharedMaterial;
                    mRend.sharedMaterial.SetTexture("_MainTex", sRend.sprite.texture);
				}
				frag.SetActive(true);
            }
			if ( fragmentTxs.Count != fragments.Count )
			{
				fragmentTxs.Clear ();
				foreach ( var frag in fragments )
					fragmentTxs.Add ( ( frag.transform.localPosition , frag.transform.localRotation ) );
			}
		}
		//if fragments exist destroy the original
		if (fragments.Count > 0)
        {
            var renderer = gameObject.GetComponent<SpriteRenderer>();
            var colider = gameObject.GetComponent<BoxCollider2D>();

			renderer.enabled = false;
			colider.enabled = false;
		}
    }
    /// <summary>
    /// Creates fragments and then disables them
    /// </summary>
    public void fragmentInEditor()
    {
        if (fragments.Count > 0)
        {
            deleteFragments();
        }
        generateFragments();
        setPolygonsForDrawing();
        foreach (GameObject frag in fragments)
        {
            frag.transform.parent = transform;
            frag.SetActive(false);
        }
    }
    public void deleteFragments()
    {
        foreach (GameObject frag in fragments)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(frag);
            }
            else
            {
                Destroy(frag);
            }
        }
        fragments.Clear();
		fragmentTxs.Clear ();
        polygons.Clear();

		resetSingle ();
    }

	public void resetFragments ()
	{
		for ( int i = 0 ; i < fragments.Count ; i++ )
		{
			var fragment = fragments [ i ];
			var fragmentTx = fragmentTxs [ i ];
			fragment.transform.localPosition = fragmentTx.pos;
			fragment.transform.localRotation = fragmentTx.rot;
			fragment.SetActive ( false );
		}
		resetSingle ();
	}

	private void resetSingle ()
	{
		var renderer = gameObject.GetComponent<SpriteRenderer> ();
		var colider = gameObject.GetComponent<BoxCollider2D> ();

		renderer.enabled = true;
		colider.enabled = true;
	}

	public void fragmentInEditor_meshSave()
    {
        if (fragments.Count > 0)
        {
            deleteFragments();
        }
        generateFragments(true);
        setPolygonsForDrawing();

        foreach (GameObject frag in fragments)
        {
            frag.transform.parent = transform;
            frag.SetActive(false);
        }
    }

    /// <summary>
    /// Turns Gameobject into multiple fragments
    /// </summary>
    private void generateFragments(bool meshSaved = false)
    {
#if UNITY_EDITOR
		string savePath = null;
		if ( meshSaved )
		{
			savePath = EditorUtility.SaveFolderPanel ( "Save fracture pieces" , "Assets/Mesh" , "" );
			if ( !string.IsNullOrEmpty ( savePath ) )
			{
				savePath = savePath.Substring ( savePath.IndexOf ( "Assets/" , StringComparison.OrdinalIgnoreCase ) );
				Directory.CreateDirectory ( savePath );
			}
			else
			{
				return;
			}
		}
#endif

		fragments = new List<GameObject>();

        switch (shatterType)
        {
            case ShatterType.Triangle:
                fragments = SpriteExploder.GenerateTriangularPieces(gameObject, extraPoints, subshatterSteps, null, meshSaved);
                break;
            case ShatterType.Voronoi:
                fragments = SpriteExploder.GenerateVoronoiPieces(gameObject, extraPoints, subshatterSteps, null, meshSaved);
                break;
            default:
                Debug.Log("invalid choice");
                break;
        }


		fragmentTxs.Clear ();
		foreach ( var fragment in fragments )
		{
			if (fragment != null)
			{
				fragment.layer                                     = LayerMask.NameToLayer(fragmentLayer);
				fragment.GetComponent<Renderer>().sortingLayerName = sortingLayerName;
				fragment.GetComponent<Renderer>().sortingOrder     = orderInLayer;
				fragmentTxs.Add ( ( fragment.transform.localPosition , fragment.transform.localRotation ) );
			}
		}

#if UNITY_EDITOR
		if ( meshSaved )
		{
			foreach ( var mesh in fragments.Select ( f => f.GetComponent <MeshFilter> () ) )
				AssetDatabase.CreateAsset ( mesh , savePath + '/' + transform.name + "_" + mesh.transform.GetSiblingIndex () + ".asset" );
			AssetDatabase.SaveAssets ();
		}
#endif

		foreach ( ExplodableAddon addon in GetComponents<ExplodableAddon>())
        {
            if (addon.enabled)
            {
                addon.OnFragmentsGenerated(fragments);
            }
        }
    }
    private void setPolygonsForDrawing()
    {
        polygons.Clear();
        List<Vector2> polygon;

        foreach (GameObject frag in fragments)
        {
            polygon = new List<Vector2>();
            foreach (Vector2 point in frag.GetComponent<PolygonCollider2D>().points)
            {
                Vector2 offset = rotateAroundPivot((Vector2)frag.transform.position, (Vector2)transform.position, Quaternion.Inverse(transform.rotation)) - (Vector2)transform.position;
                offset.x /= transform.localScale.x;
                offset.y /= transform.localScale.y;
                polygon.Add(point + offset);
            }
            polygons.Add(polygon);
        }
    }
    private Vector2 rotateAroundPivot(Vector2 point, Vector2 pivot, Quaternion angle)
    {
        Vector2 dir = point - pivot;
        dir = angle * dir;
        point = dir + pivot;
        return point;
    }

    void OnDrawGizmos()
    {
        if (Application.isEditor)
        {
            if (polygons.Count == 0 && fragments.Count != 0)
            {
                setPolygonsForDrawing();
            }

            Gizmos.color = Color.blue;
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector2 offset = (Vector2)transform.position * 0;
            foreach (List<Vector2> polygon in polygons)
            {
                for (int i = 0; i < polygon.Count; i++)
                {
                    if (i + 1 == polygon.Count)
                    {
                        Gizmos.DrawLine(polygon[i] + offset, polygon[0] + offset);
                    }
                    else
                    {
                        Gizmos.DrawLine(polygon[i] + offset, polygon[i + 1] + offset);
                    }
                }
            }
        }
    }
}
