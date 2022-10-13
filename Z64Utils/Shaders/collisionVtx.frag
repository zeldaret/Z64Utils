#version 330 core

in vec4 v_VtxColor;

out vec4 FragColor;

void main()
{
	FragColor = v_VtxColor;
}