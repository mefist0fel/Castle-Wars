using UnityEngine;
using System.Collections.Generic;

public sealed class MeshGenerator {
	private readonly string name;
	private List<Vector3> vertices = new List<Vector3> ();
	private List<Vector3> normals = new List<Vector3> ();
	private List<Color> colors = new List<Color>();
	private List<Vector2> textureCoords = new List<Vector2> ();
	private List<int> faces = new List<int> ();
	private int vertexId = 0;

	public MeshGenerator(string meshName = "") {
		name = meshName;
	}

	public void AddQuad( Vector3 vertexTL, Vector3 vertexTR, Vector3 vertexBR, Vector3 vertexBL) {
		var normal = (
			Utils.GetNormal (vertexTL, vertexTR, vertexBR) * 0.5f +
			Utils.GetNormal (vertexTL, vertexBR, vertexBL) * 0.5f
		).normalized;
		AddQuad (vertexTL, vertexTR, vertexBR, vertexBL, normal);
	}

	public void AddQuad(
		Vector3 vertexTL, Vector3 vertexTR, Vector3 vertexBR, Vector3 vertexBL,
		Vector3 normal ) {
		AddQuad (
			vertexTL, vertexTR, vertexBR, vertexBL,
			normal, normal, normal, normal,
			Color.white, Color.white, Color.white, Color.white);
	}

	public void AddColoredQuad(
		Vector3 vertexTL, Vector3 vertexTR, Vector3 vertexBR, Vector3 vertexBL,
		Vector3 normal, Color color)
	{
		AddQuad(
			vertexTL, vertexTR, vertexBR, vertexBL,
			normal, normal, normal, normal,
			color, color, color, color);
	}
	public void AddQuad(
		Vector3 vertexTL, Vector3 vertexTR, Vector3 vertexBR, Vector3 vertexBL,
		Vector3 normalTL, Vector3 normalTR, Vector3 normalBR, Vector3 normalBL
	)
	{
		AddQuad(
			vertexTL, vertexTR, vertexBR, vertexBL,
			normalTL, normalTR, normalBR, normalBL,
			Color.white, Color.white, Color.white, Color.white);
	}

	public void AddQuad(
		Vector3 vertexTL, Vector3 vertexTR, Vector3 vertexBR, Vector3 vertexBL,
		Vector3 normalTL, Vector3 normalTR, Vector3 normalBR, Vector3 normalBL,
		Color colorTL, Color colorTR, Color colorBR, Color colorBL
	) {
		vertices.Add (vertexTL);
		vertices.Add (vertexTR);
		vertices.Add (vertexBR);
		vertices.Add (vertexBL);
		textureCoords.Add (new Vector2(0, 0));
		textureCoords.Add (new Vector2(0, 1));
		textureCoords.Add (new Vector2(1, 1));
		textureCoords.Add (new Vector2(1, 0));
		normals.Add (normalTL);
		normals.Add (normalTR);
		normals.Add (normalBR);
		normals.Add (normalBL);
		colors.Add(colorTL);
		colors.Add(colorTR);
		colors.Add(colorBR);
		colors.Add(colorBL);
		faces.Add (vertexId + 0);
		faces.Add (vertexId + 1);
		faces.Add (vertexId + 2);
		faces.Add (vertexId + 0);
		faces.Add (vertexId + 2);
		faces.Add (vertexId + 3);
		vertexId += 4;
	}

	public void AddTexturedQuad(
		Vector3 vertexTL, Vector3 vertexTR, Vector3 vertexBR, Vector3 vertexBL,
		Vector3 normalTL, Vector3 normalTR, Vector3 normalBR, Vector3 normalBL,
		Vector2 textureTL, Vector2 textureTR, Vector2 textureBR, Vector2 textureBL
	)
	{
		vertices.Add(vertexTL);
		vertices.Add(vertexTR);
		vertices.Add(vertexBR);
		vertices.Add(vertexBL);
		textureCoords.Add(textureTL);
		textureCoords.Add(textureTR);
		textureCoords.Add(textureBR);
		textureCoords.Add(textureBL);
		normals.Add(normalTL);
		normals.Add(normalTR);
		normals.Add(normalBR);
		normals.Add(normalBL);
		colors.Add(Color.white);
		colors.Add(Color.white);
		colors.Add(Color.white);
		colors.Add(Color.white);
		faces.Add(vertexId + 0);
		faces.Add(vertexId + 1);
		faces.Add(vertexId + 2);
		faces.Add(vertexId + 0);
		faces.Add(vertexId + 2);
		faces.Add(vertexId + 3);
		vertexId += 4;
	}

	public void AddTriangle(
		Vector3 vertexA,
		Vector3 vertexB,
		Vector3 vertexC
	) {
		var normal = Utils.GetNormal (vertexA, vertexB, vertexC).normalized;
		AddTriangle (vertexA, vertexB, vertexC, normal, normal, normal);
	}

	public void AddTriangle(
		Vector3 vertexA,
		Vector3 vertexB,
		Vector3 vertexC,
		Vector3 normalA,
		Vector3 normalB,
		Vector3 normalC
	) {
		vertices.Add (vertexA);
		vertices.Add (vertexB);
		vertices.Add (vertexC);
		textureCoords.Add (new Vector2(0, 0));
		textureCoords.Add (new Vector2(0, 1));
		textureCoords.Add (new Vector2(1, 1));
		normals.Add (normalA);
		normals.Add (normalB);
		normals.Add (normalC);
		colors.Add(Color.white);
		colors.Add(Color.white);
		colors.Add(Color.white);
		faces.Add (vertexId + 0);
		faces.Add (vertexId + 1);
		faces.Add (vertexId + 2);
		vertexId += 3;
	}

	public void AddTriangle(
		Vector3 vertexA,
		Vector3 vertexB,
		Vector3 vertexC,
		Vector3 normalA,
		Vector3 normalB,
		Vector3 normalC,
		Vector2 uvA,
		Vector2 uvB,
		Vector2 uvC
	)
	{
		vertices.Add(vertexA);
		vertices.Add(vertexB);
		vertices.Add(vertexC);
		textureCoords.Add(uvA);
		textureCoords.Add(uvB);
		textureCoords.Add(uvC);
		normals.Add(normalA);
		normals.Add(normalB);
		normals.Add(normalC);
		colors.Add(Color.white);
		colors.Add(Color.white);
		colors.Add(Color.white);
		faces.Add(vertexId + 0);
		faces.Add(vertexId + 1);
		faces.Add(vertexId + 2);
		vertexId += 3;
	}

	public Mesh Generate(bool needUV = true, bool needColor = false) {
		var mesh = new Mesh();
		mesh.SetVertices(vertices);
		mesh.SetNormals(normals);
		if (needUV)
			mesh.SetUVs(0, textureCoords);
		if (needColor)
			mesh.SetColors(colors);
		mesh.SetTriangles(faces, 0);
		return mesh;
	}
}