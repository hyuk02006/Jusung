
--�Է¹��� �����ȣ�� ��� Ŀ�̼��� �����ϴ� ���α׷�(���̺� comm n(10) �÷� �߰� �� �۾�)
--��, Ŀ�̼��� �޿��� 1000�̸�	-> �޿��� 10%
	       	--   1000 ~ 2000 	-> �޿��� 15%
		  --  2000�ʰ�	-> �޿��� 20%
		--	null	-> 0
set serveroutput on
accept num prompt '�Է�->'
set verify off

declare 
	v_sapay sawon.sapay%type;
	v_comm sawon.comm%type;
begin
	select sapay into v_sapay from sawon where sabun = &num;

	if (v_sapay < 1000) then
		--update sawon set comm = v_sapay*0.1 where sabun = &num;
		v_comm:=v_sapay*0.1;
	elsif ( v_sapay <= 2000)  then
		--update sawon set comm = v_sapay*0.15 where sabun = &num;
		v_comm:=v_sapay*0.15;
	elsif(v_sapay > 2000) then
		--update sawon set comm = v_sapay*0.2 where sabun = &num;	
		v_comm:=v_sapay*0.2;
	else
		--update sawon set comm = null where sabun = &num;	
		v_comm:=0;
	end if;

	update sawon set comm =v_comm where sabun =&num;
	
end;
/