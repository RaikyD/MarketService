import React, { useState, useEffect } from 'react'
import axios from 'axios'
import { ToastContainer, toast } from 'react-toastify'
import { HubConnectionBuilder } from '@microsoft/signalr'
import 'react-toastify/dist/ReactToastify.css'

const API = process.env.REACT_APP_API || 'http://localhost:8080'
const STATUS_MAP = { 0:'New',1:'Finished',2:'Canceled' }
const isGuid = s => /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(s)

export default function App() {
  const [initAmt,  setInitAmt]  = useState('')
  const [users,    setUsers]    = useState([])
  const [selUser,  setSelUser]  = useState('')
  const [topUp,    setTopUp]    = useState('')

  // — Сервис заказов —
  const [orders,   setOrders]   = useState([])
  const [oUser,    setOUser]    = useState('')
  const [oAmt,     setOAmt]     = useState('')
  const [oDesc,    setODesc]    = useState('')
  const [statusId, setStatusId] = useState('')
  const [orderState, setOrderState] = useState('')
  const [hub,      setHub]      = useState(null)

  // сохраняем/загружаем users в localStorage
  useEffect(() => {
    localStorage.clear();
    const stored = localStorage.getItem('users')
    if (stored) setUsers(JSON.parse(stored))
  }, [])
  useEffect(() => {
    localStorage.setItem('users', JSON.stringify(users))
  }, [users])

  // запрос прав на desktop-нотификации
  useEffect(() => {
    if ('Notification' in window && Notification.permission !== 'granted') {
      Notification.requestPermission()
    }
  }, [])

  // один раз при старте: загрузить заказы и поднять SignalR
  useEffect(() => {
    axios.get(`${API}/GetAllOrders`)
      .then(resp => {
        setOrders(resp.data)
        const conn = new HubConnectionBuilder()
          .withUrl(`${API}/orderHub`, { withCredentials: true })
          .withAutomaticReconnect()
          .build()
        conn.start().then(() => {
          // подписаться на группы
          resp.data.forEach(o => conn.invoke('JoinOrder', o.id))
          conn.on('StatusChanged', (id, code) => {
            const txt = STATUS_MAP[code] || code
            toast.info(`Order ${id} → ${txt}`)
            if (Notification.permission === 'granted') {
              new Notification('Order status changed', { body: `${id} → ${txt}` })
            }
            setOrders(os => os.map(o => o.id === id ? { ...o, status: code } : o))
            if (id === statusId) setOrderState(txt)
          })
        })
        setHub(conn)
      })
      .catch(() => toast.error('Не удалось загрузить заказы'))
  }, [statusId])

  // ===== Платёжный сервис =====

  const createAccount = async () => {
    if (isNaN(+initAmt)||+initAmt<0){ toast.error('Σ≥0'); return }
    try {
      const { data } = await axios.post(`${API}/Payment?amount=${initAmt}`)
      setUsers(u=>[...u, {id:data.userId,balance:data.amount}])
      toast.success(`Счёт: ${data.userId}`)
    } catch { toast.error('Ошибка') }
  }

  const fetchBalance = async () => {
    if (!isGuid(selUser)){ toast.error('ID'); return }
    try {
      const { data } = await axios.get(`${API}/Payment?userId=${selUser}`)
      setUsers(u=>u.map(x=>x.id===selUser?{...x,balance:data}:x))
      toast.success(`Баланс: ${data}`)
    } catch { toast.error('Ошибка') }
  }

  const doTopUp = async () => {
    if (!isGuid(selUser)||+topUp<=0){ toast.error('Проверьте'); return }
    try {
      const { data } = await axios.put(
        `${API}/Payment?userId=${selUser}&amount=${topUp}`)
      setUsers(u=>u.map(x=>x.id===selUser?{...x,balance:data}:x))
      toast.success(`Пополнено: ${data}`)
    } catch { toast.error('Ошибка') }
  }

  // const createOrder = async () => {
  //   if (!isGuid(oUser) || +oAmt <= 0) { toast.error('Проверьте'); return }
  //   try {
  //     const { data } = await axios.post(
  //       `${API}/Order?userId=${oUser}&amount=${oAmt}&description=${encodeURIComponent(oDesc)}`
  //     )
  //     setOrders(o => [...o, { ...data, status: 0 }])
  //     hub?.invoke('JoinOrder', data.id)
  //     toast.success(`Заказ: ${data.id}`)
  
  //     // Получаем статус сразу (без setTimeout)
  //     const { data: statusData } = await axios.get(`${API}/GetOrderStatus?id=${data.id}`)
  //     const statusText = STATUS_MAP[statusData] || statusData
  //     setOrderState(statusText)
  //     toast.success(`Статус: ${statusText}`)
  
  //     setTimeout(async () => {
  //       const { data: newStatus } = await axios.get(`${API}/GetOrderStatus?id=${data.id}`)
  //       const newStatusText = STATUS_MAP[newStatus] || newStatus
  //       setOrderState(newStatusText)
  //       toast.info(`Статус: ${newStatusText} заказа ${data.id}`)
  //     }, 7000)
  //   } catch { toast.error('Ошибка') }
  // }
  const createOrder = async () => {
    if (!isGuid(oUser) || +oAmt <= 0) {
      toast.error('Проверьте ввод');
      return;
    }
  
    try {
      const { data: order } = await axios.post(
        `${API}/Order?userId=${oUser}&amount=${oAmt}&description=${encodeURIComponent(oDesc)}`
      );
      toast.success(`Заказ создан: ${order.id}`);
      setOrders(o => [...o, { ...order, status: 0 }]);
      const pollInterval = 3000;
      const timer = setInterval(async () => {
        try {
          const { data: statusCode } = await axios.get(
            `${API}/GetOrderStatus?id=${order.id}`
          );
          console.log(statusCode);
          if (statusCode !== "New") {
            const text = STATUS_MAP[statusCode] || statusCode;
            toast.success(`Заказ ${order.id} перешёл в статус: ${text}`);
            setOrderState(text);
            clearInterval(timer);
          }
        } catch {
          clearInterval(timer);
          toast.error('Не удалось опросить статус заказа');
        }
      }, pollInterval);
  
    } catch {
      toast.error('Ошибка при создании заказа');
    }
  };
  

  const fetchStatus = async () => {
    if (!isGuid(statusId)){ toast.error('ID'); return }
    try {
      const { data } = await axios.get(`${API}/GetOrderStatus?id=${statusId}`)
      const txt = STATUS_MAP[data] || data
      setOrderState(txt)
      toast.success(`Статус: ${txt}`)
    } catch { toast.error('Ошибка') }
  }

  return (
    <div style={{padding:20,fontFamily:'sans-serif'}}>
      <h2>Платёжный сервис</h2>
      <div style={{display:'flex',gap:8,flexWrap:'wrap',marginBottom:16}}>
        <input
          style={{width:120}}
          placeholder="Начальная сумма"
          value={initAmt} onChange={e=>setInitAmt(e.target.value)}
        />
        <button onClick={createAccount}>Создать счёт</button>

        <select
          style={{minWidth:200}}
          value={selUser} onChange={e=>setSelUser(e.target.value)}
        >
          <option value="">— выберите User —</option>
          {users.map(u=>
            <option key={u.id} value={u.id}>
              {u.id.slice(0,8)}… (bal:{u.balance})
            </option>
          )}
        </select>
        <button onClick={fetchBalance}>Баланс</button>

        <input
          style={{width:100}}
          placeholder="Пополнить"
          value={topUp} onChange={e=>setTopUp(e.target.value)}
        />
        <button onClick={doTopUp}>Пополнить</button>
      </div>

      <hr/>

      <h2>Сервис заказов</h2>
      <div style={{display:'flex',gap:8,flexWrap:'wrap',marginBottom:16}}>
        <select
          style={{minWidth:200}}
          value={oUser} onChange={e=>setOUser(e.target.value)}
        >
          <option value="">— User ID —</option>
          {users.map(u=>
            <option key={u.id} value={u.id}>{u.id.slice(0,8)}</option>
          )}
        </select>
        <input
          style={{width:100}}
          placeholder="Сумма"
          value={oAmt} onChange={e=>setOAmt(e.target.value)}
        />
        <input
          style={{width:200}}
          placeholder="Описание"
          value={oDesc} onChange={e=>setODesc(e.target.value)}
        />
        <button onClick={createOrder}>Создать заказ</button>

        {}
        <input
          style={{width:200}}
          placeholder="Order ID для статуса"
          value={statusId} onChange={e=>setStatusId(e.target.value)}
        />
        <button onClick={fetchStatus}>Получить статус</button>
        {orderState && <span style={{marginLeft:8}}>→ {orderState}</span>}
      </div>

      <h3>Все заказы</h3>
      <table
        border="1" cellPadding="6"
        style={{width:'100%',borderCollapse:'collapse'}}
      >
        <thead>
          <tr>
            <th style={{width:'20%'}}>Order ID</th>
            <th style={{width:'20%'}}>User ID</th>
            <th style={{width:'10%'}}>Amount</th>
            <th style={{width:'25%'}}>Description</th>
            <th style={{width:'15%'}}>Status</th>
          </tr>
        </thead>
        <tbody>
          {orders.map(o=>(
            <tr key={o.id}>
              <td style={{fontSize:12}}>{o.id}</td>
              <td style={{fontSize:12}}>{o.userId}</td>
              <td>{o.amount}</td>
              <td>{o.description}</td>
              <td>{STATUS_MAP[o.status]}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <ToastContainer position="top-right"/>
    </div>
  )
}
