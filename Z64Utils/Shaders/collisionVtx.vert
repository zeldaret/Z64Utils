#version 330 core

layout (location = 0) in vec3 pos;
layout (location = 1) in vec3 normal;

uniform vec4 u_Color;
uniform mat4 u_Projection;
uniform mat4 u_View;
uniform mat4 u_Model;

out vec4 v_VtxColor;

void main()
{
	gl_Position = u_Projection * u_View * u_Model * vec4(pos, 1);
	v_VtxColor = u_Color * (0.3 + dot(u_View * vec4(normal,0), vec4(0,0,0.7,0)));
}